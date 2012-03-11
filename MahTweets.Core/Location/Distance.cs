namespace MahTweets.Core.Location
{
    public struct Distance
    {
        private readonly DistanceUnit _unit;
        private readonly double _value;

        public Distance(double value, DistanceUnit unit)
        {
            _value = value;
            _unit = unit;
        }

        public override string ToString()
        {
            if (_value < 1.0)
                return ", very near";

            // TODO: parse distance unit value into string
            // use Description attribute on _unit
            // requires a bit of reflection-fu
            const string enumName = "kms";

            return string.Format(", {0:#,0} {1} away", _value, enumName);
        }
    }
}