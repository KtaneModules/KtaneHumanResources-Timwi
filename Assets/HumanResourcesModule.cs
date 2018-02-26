using System.Collections;
using System.Collections.Generic;
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
    private TextState _nameState = new TextState();
    private TextState _descState = new TextState();

    private Person[] _people =
    {
        new Person { Name = "Rebecca", MBTI = "INTJ", Descriptor = "Intellectual" },
        new Person { Name = "Damian", MBTI = "INTP", Descriptor = "Deviser" },
        new Person { Name = "Jean", MBTI = "INFJ", Descriptor = "Confident" },
        new Person { Name = "Mike", MBTI = "INFP", Descriptor = "Helper" },
        new Person { Name = "River", MBTI = "ISTJ", Descriptor = "Auditor" },
        new Person { Name = "Samuel", MBTI = "ISTP", Descriptor = "Innovator" },
        new Person { Name = "Yoshi", MBTI = "ISFJ", Descriptor = "Defender" },
        new Person { Name = "Caleb", MBTI = "ISFP", Descriptor = "Chameleon" },
        new Person { Name = "Ashley", MBTI = "ENTJ", Descriptor = "Director" },
        new Person { Name = "Tim", MBTI = "ENTP", Descriptor = "Designer" },
        new Person { Name = "Eliott", MBTI = "ENFJ", Descriptor = "Educator" },
        new Person { Name = "Ursula", MBTI = "ENFP", Descriptor = "Advocate" },
        new Person { Name = "Silas", MBTI = "ESTJ", Descriptor = "Manager" },
        new Person { Name = "Noah", MBTI = "ESTP", Descriptor = "Showman" },
        new Person { Name = "Quinn", MBTI = "ESFJ", Descriptor = "Contributor" },
        new Person { Name = "Dylan", MBTI = "ESFP", Descriptor = "Entertainer" }
    };

    private int[] _availableNames;
    private int[] _availableDescs;

    private int _nameIndex;
    private int _descIndex;

    private int _personToFire;
    private int _personToHire;

    private bool _correctFired = false;
    private bool _isSolved = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    private static string _green = "48E64F";
    private static string _red = "E63B5E";

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        ButtonLeftNames.OnInteract += NamesCycleLeft;
        ButtonRightNames.OnInteract += NamesCycleRight;
        ButtonLeftDescs.OnInteract += DescsCycleLeft;
        ButtonRightDescs.OnInteract += DescsCycleRight;
        ButtonHire.OnInteract += Hire;
        ButtonFire.OnInteract += Fire;

        tryAgain:

        // Choose 10 people and 5 descriptors
        _availableNames = Enumerable.Range(0, _people.Length).ToList().Shuffle().Take(10).ToArray();
        _availableDescs = Enumerable.Range(0, _people.Length).ToList().Shuffle().Take(5).ToArray();

        var personToFire = FindPerson(_availableNames.Take(5), _availableDescs.Take(3));
        if (personToFire == null)
            goto tryAgain;
        _personToFire = personToFire.PersonIndex;

        var personToHire = FindPerson(_availableNames.Skip(5), _availableDescs.Skip(3).Concat(new[] { _personToFire }));
        if (personToHire == null)
            goto tryAgain;
        _personToHire = personToHire.PersonIndex;

        Debug.LogFormat("[Human Resources #{0}] Complaints: {1}", _moduleId, _availableDescs.Take(3).Select(ix => string.Format("{0} ({1})", _people[ix].Descriptor, _people[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Required: {1}, preferred: {2}", _moduleId,
            personToFire.Required.Length == 0 ? "(none)" : personToFire.Required.JoinString("+"),
            personToFire.Preferred.Length == 0 ? "(none)" : personToFire.Preferred.JoinString("+"));
        Debug.LogFormat("[Human Resources #{0}] Employees: {1}", _moduleId, _availableNames.Take(5).Select(ix => string.Format("{0} ({1})", _people[ix].Name, _people[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Person to fire: {1} ({2})", _moduleId, _people[_personToFire].Name, _people[_personToFire].MBTI);
        Debug.LogFormat("[Human Resources #{0}] Fired person adds desired trait: {1}", _moduleId, _people[_personToFire].Descriptor);

        Debug.LogFormat("[Human Resources #{0}] Desired traits: {1}", _moduleId, _availableDescs.Skip(3).Concat(new[] { _personToFire }).Select(ix => string.Format("{0} ({1})", _people[ix].Descriptor, _people[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Required: {1}, preferred: {2}", _moduleId,
            personToHire.Required.Length == 0 ? "(none)" : personToHire.Required.JoinString("+"),
            personToHire.Preferred.Length == 0 ? "(none)" : personToHire.Preferred.JoinString("+"));
        Debug.LogFormat("[Human Resources #{0}] Applicants: {1}", _moduleId, _availableNames.Skip(5).Select(ix => string.Format("{0} ({1})", _people[ix].Name, _people[ix].MBTI)).JoinString(", "));
        Debug.LogFormat("[Human Resources #{0}] Person to hire: {1} ({2})", _moduleId, _people[_personToHire].Name, _people[_personToHire].MBTI);

        _nameIndex = Rnd.Range(0, _availableNames.Length);
        setName();
        _descIndex = Rnd.Range(0, _availableDescs.Length);
        setDesc();

        StartCoroutine(textCoroutine(NamesText, _nameState));
        StartCoroutine(textCoroutine(DescsText, _descState));
    }

    private void setName()
    {
        setText(_nameState, _people[_availableNames[_nameIndex]].Name, _nameIndex < 5 ? _green : _red);
    }

    private void setDesc()
    {
        setText(_descState, _people[_availableDescs[_descIndex]].Descriptor, _descIndex < 3 ? _red : _green);
    }

    private void setText(TextState state, string newText, string newColor)
    {
        if (!state.CurrentlyDeleting)
        {
            state.DelText = state.InsText;
            state.DelColor = state.InsColor;
            state.CurrentlyDeleting = state.CurIndex > 0;
        }
        state.InsText = newText;
        state.InsColor = newColor;
    }

    private IEnumerator textCoroutine(TextMesh mesh, TextState state)
    {
        while (true)
        {
            yield return new WaitForSeconds(.05f);

            if (state.CurrentlyDeleting)
            {
                state.CurIndex--;
                mesh.text = string.Format("<color=#{0}>{1}</color><color=#D1D225>█</color>", state.DelColor, state.DelText.Substring(0, state.CurIndex));
                if (state.CurIndex == 0)
                    state.CurrentlyDeleting = false;
                Audio.PlaySoundAtTransform("beep_short", mesh.transform);
            }
            else if (state.CurIndex < state.InsText.Length)
            {
                state.CurIndex++;
                if (state.CurIndex < state.InsText.Length)
                    mesh.text = string.Format("<color=#{0}>{1}</color><color=#D1D225>█</color>", state.InsColor, state.InsText.Substring(0, state.CurIndex));
                else
                    mesh.text = string.Format("<color=#{0}>{1}</color>", state.InsColor, state.InsText);
                Audio.PlaySoundAtTransform("beep_short", mesh.transform);
            }
        }
    }

    private FindPersonResult FindPerson(IEnumerable<int> names, IEnumerable<int> descs)
    {
        var required = "EINSFTJP".Where(ch => descs.All(ix => _people[ix].MBTI.Contains(ch))).ToArray();
        var preferred = ("EINSFTJP".Except(required)).Where(ch => descs.Count(ix => _people[ix].MBTI.Contains(ch)) == 2).ToArray();
        var peopleInfos = names.Select(ix => new
        {
            Index = ix,
            RequiredCount = required.Count(ch => _people[ix].MBTI.Contains(ch)),
            PreferredCount = preferred.Count(ch => _people[ix].MBTI.Contains(ch))
        }).OrderByDescending(info => info.RequiredCount).ToArray();

        if (peopleInfos[0].RequiredCount > peopleInfos[1].RequiredCount)
            // No tie!
            return new FindPersonResult(peopleInfos[0].Index, required, preferred);

        // Number of required traits is tied; look at number of preferred traits
        var candidates = peopleInfos.Where(info => info.RequiredCount == peopleInfos[0].RequiredCount).OrderByDescending(info => info.PreferredCount).ToArray();
        if (candidates[0].PreferredCount > candidates[1].PreferredCount)
            // No tie this time!
            return new FindPersonResult(candidates[0].Index, required, preferred);

        // It’s still a tie; try again!
        return null;
    }

    private bool DescsCycleLeft()
    {
        ButtonLeftDescs.AddInteractionPunch(.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonLeftDescs.transform);

        if (_isSolved)
            return false;

        _descIndex = ((_descIndex - 1) + _availableDescs.Length) % _availableDescs.Length;
        setDesc();

        return false;
    }

    private bool DescsCycleRight()
    {
        ButtonRightDescs.AddInteractionPunch(.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonRightDescs.transform);

        if (_isSolved)
            return false;

        _descIndex = (_descIndex + 1) % _availableDescs.Length;
        setDesc();

        return false;
    }

    private bool NamesCycleLeft()
    {
        ButtonLeftNames.AddInteractionPunch(.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonLeftNames.transform);

        if (_isSolved)
            return false;

        _nameIndex = ((_nameIndex - 1) + _availableNames.Length) % _availableNames.Length;
        setName();

        return false;
    }

    private bool NamesCycleRight()
    {
        ButtonRightNames.AddInteractionPunch(.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ButtonRightNames.transform);

        if (_isSolved)
            return false;

        _nameIndex = (_nameIndex + 1) % _availableNames.Length;
        setName();

        return false;
    }

    private bool Fire()
    {
        ButtonFire.AddInteractionPunch();
        Audio.PlaySoundAtTransform("beep_long", ButtonFire.transform);

        if (_isSolved)
            return false;

        Debug.LogFormat("[Human Resources #{0}] Chose to fire: {1} ({2})", _moduleId, _people[_availableNames[_nameIndex]].Name, _availableNames[_nameIndex] == _personToFire ? "correct" : "wrong");

        if (_availableNames[_nameIndex] == _personToFire)
            _correctFired = true;
        else
            Module.HandleStrike();

        return false;
    }

    private bool Hire()
    {
        ButtonHire.AddInteractionPunch();
        Audio.PlaySoundAtTransform("beep_long", ButtonHire.transform);

        if (_isSolved)
            return false;

        Debug.LogFormat("[Human Resources #{0}] Chose to hire: {1} ({2})", _moduleId, _people[_availableNames[_nameIndex]].Name, _availableNames[_nameIndex] == _personToHire ? (_correctFired ? "correct" : "correct, but need to fire first") : "wrong");

        if (_availableNames[_nameIndex] == _personToHire && _correctFired)
        {
            Debug.LogFormat("[Human Resources #{0}] Module solved.", _moduleId);
            _isSolved = true;
            Module.HandlePass();
        }
        else
            Module.HandleStrike();

        return false;
    }
}
