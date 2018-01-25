using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HumanResources;
using UnityEngine;

using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Human Resources
/// Created by Elias8885, Timwi and Skyeward
/// </summary>
public class HumanResourcesModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;

    public KMSelectable ButtonLeftNames;
    public KMSelectable ButtonRightNames;
    public KMSelectable ButtonLeftDescs;
    public KMSelectable ButtonRightDescs;
    public KMSelectable ButtonHire;
    public KMSelectable ButtonFire;

    public TextMesh NamesText;
    public TextMesh DescsText;

    private Person[] People =
    {
        new Person{Name = "Rebecca", MBTI = "INTJ", Descriptor = "Intellectual"},
        new Person{Name = "Damian", MBTI = "INTP", Descriptor = "Deviser"},
        new Person{Name = "Jean", MBTI = "INFJ", Descriptor = "Confident"},
        new Person{Name = "Mike", MBTI = "INFP", Descriptor = "Helper"},
        new Person{Name = "River", MBTI = "ISTJ", Descriptor = "Auditor"},
        new Person{Name = "Samuel", MBTI = "ISTP", Descriptor = "Innovator"},
        new Person{Name = "Yoshi", MBTI = "ISFJ", Descriptor = "Defender"},
        new Person{Name = "Caleb", MBTI = "ISFP", Descriptor = "Chameleon"},
        new Person{Name = "Ashley", MBTI = "ENTJ", Descriptor = "Director"},
        new Person{Name = "Tim", MBTI = "ENTP", Descriptor = "Designer"},
        new Person{Name = "Eliott", MBTI = "ENFJ", Descriptor = "Educator"},
        new Person{Name = "Ursula", MBTI = "ENFP", Descriptor = "Advocate"},
        new Person{Name = "Silas", MBTI = "ESTJ", Descriptor = "Manager"},
        new Person{Name = "Noah", MBTI = "ESTP", Descriptor = "Showman"},
        new Person{Name = "Quinn", MBTI = "ESFJ", Descriptor = "Contributor"},
        new Person{Name = "Dylan", MBTI = "ESFP", Descriptor = "Entertainer"}
    };

    private int[] AvailableNames;
    private int[] AvailableDescs;

    private int NameIndex;
    private int DescIndex;

    private int PersonToFire;
    private int PersonToHire;

    private bool CorrectFired = false;
    private bool IsSolved = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        ButtonLeftNames.OnInteract += NamesCycleLeft;
        ButtonRightNames.OnInteract += NamesCycleRight;
        ButtonLeftDescs.OnInteract += DescsCycleLeft;
        ButtonRightDescs.OnInteract += DescsCycleRight;
        ButtonHire.OnInteract += ChooseHire;
        ButtonFire.OnInteract += ChooseFire;

    tryAgain:

        // Choose 10 people and 5 descriptors
        AvailableNames = Enumerable.Range(0, People.Length).ToList().Shuffle().Take(10).ToArray();
        AvailableDescs = Enumerable.Range(0, People.Length).ToList().Shuffle().Take(5).ToArray();

        var personToFire = FindPerson(AvailableNames.Take(5), AvailableDescs.Take(3));
        if (personToFire == null)
            goto tryAgain;
        PersonToFire = personToFire.Value;

        var personToHire = FindPerson(AvailableNames.Skip(5), AvailableDescs.Skip(3).Concat(new[] { PersonToFire }));
        if (personToHire == null)
            goto tryAgain;
        PersonToHire = personToHire.Value;

        Debug.LogFormat("[Human Resources #{0}] Employees: {1}, Applicants: {2}", _moduleId,
            AvailableNames.Take(5).Select(ix => string.Format("{0} ({1})", People[ix].Name, People[ix].MBTI)).JoinString(", "),
            AvailableNames.Skip(5).Select(ix => string.Format("{0} ({1})", People[ix].Name, People[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Complaints: {1}, Desired: {2}", _moduleId,
            AvailableDescs.Take(3).Select(ix => string.Format("{0} ({1})", People[ix].Descriptor, People[ix].MBTI)).JoinString(", "),
            AvailableDescs.Skip(3).Select(ix => string.Format("{0} ({1})", People[ix].Descriptor, People[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Person to fire: {1} ({2})", _moduleId, People[PersonToFire].Name, People[PersonToFire].MBTI);
        Debug.LogFormat("[Human Resources #{0}] Person to hire: {1} ({2})", _moduleId, People[PersonToHire].Name, People[PersonToHire].MBTI);

        NameIndex = Rnd.Range(0, AvailableNames.Length);
        SetName();
        DescIndex = Rnd.Range(0, AvailableDescs.Length);
        SetDesc();
    }

    private void SetName()
    {
        NamesText.text = People[AvailableNames[NameIndex]].Name;
        NamesText.color = NameIndex < 5 ? Color.green : Color.red;
    }

    private void SetDesc()
    {
        DescsText.text = People[AvailableDescs[DescIndex]].Descriptor;
        DescsText.color = DescIndex < 3 ? Color.red : Color.green;
    }

    private int? FindPerson(IEnumerable<int> names, IEnumerable<int> descs)
    {
        var required = "EINSFTJP".Where(ch => descs.All(ix => People[ix].MBTI.Contains(ch))).ToArray();
        var preferred = ("EINSFTJP".Except(required)).Where(ch => descs.Count(ix => People[ix].MBTI.Contains(ch)) == 2).ToArray();
        var peopleInfos = names.Select(ix => new
        {
            Index = ix,
            RequiredCount = required.Count(ch => People[ix].MBTI.Contains(ch)),
            PreferredCount = preferred.Count(ch => People[ix].MBTI.Contains(ch))
        }).OrderByDescending(info => info.RequiredCount).ToArray();

        if (peopleInfos[0].RequiredCount > peopleInfos[1].RequiredCount)
            // No tie!
            return peopleInfos[0].Index;

        // Number of required traits is tied; look at number of preferred traits
        var candidates = peopleInfos.Where(info => info.RequiredCount == peopleInfos[0].RequiredCount).OrderByDescending(info => info.PreferredCount).ToArray();
        if (candidates[0].PreferredCount > candidates[1].PreferredCount)
            // No tie this time!
            return candidates[0].Index;

        // It’s still a tie; try again!
        return null;
    }

    private bool DescsCycleLeft()
    {
        if (IsSolved)
            return false;

        DescIndex = ((DescIndex - 1) + AvailableDescs.Length) % AvailableDescs.Length;
        SetDesc();

        return false;
    }

    private bool DescsCycleRight()
    {
        if (IsSolved)
            return false;

        DescIndex = (DescIndex + 1) % AvailableDescs.Length;
        SetDesc();

        return false;
    }

    private bool NamesCycleLeft()
    {
        if (IsSolved)
            return false;

        NameIndex = ((NameIndex - 1) + AvailableNames.Length) % AvailableNames.Length;
        SetName();

        return false;
    }

    private bool NamesCycleRight()
    {
        if (IsSolved)
            return false;

        NameIndex = (NameIndex + 1) % AvailableNames.Length;
        SetName();

        return false;
    }

    private bool ChooseFire()
    {
        if (IsSolved)
            return false;

        Debug.LogFormat("[Human Resources #{0}] Chose to fire: {1} ({2})", _moduleId, People[AvailableNames[NameIndex]].Name, AvailableNames[NameIndex] == PersonToFire ? "correct" : "wrong");

        if (AvailableNames[NameIndex] == PersonToFire)
        {
            CorrectFired = true;
        }
        else
        {
            Module.HandleStrike();
        }

        return false;
    }

    private bool ChooseHire()
    {
        if (IsSolved)
            return false;

        Debug.LogFormat("[Human Resources #{0}] Chose to hire: {1} ({2})", _moduleId, People[AvailableNames[NameIndex]].Name, AvailableNames[NameIndex] == PersonToHire ? (CorrectFired ? "correct" : "correct, but need to fire first") : "wrong");

        if (AvailableNames[NameIndex] == PersonToHire && CorrectFired)
        {
            Debug.LogFormat("[Human Resources #{0}] Module solved.", _moduleId);
            IsSolved = true;
            Module.HandlePass();
        }
        else
        {
            Module.HandleStrike();
        }

        return false;
    }
}
