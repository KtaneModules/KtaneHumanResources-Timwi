namespace HumanResources
{
    sealed class FindPersonResult
    {
        public int PersonIndex { get; private set; }
        public char[] Required { get; private set; }
        public char[] Preferred { get; private set; }
        public FindPersonResult(int personIndex, char[] required, char[] preferred)
        {
            PersonIndex = personIndex;
            Required = required;
            Preferred = preferred;
        }
    }
}