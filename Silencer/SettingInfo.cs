namespace Silencer
{
    public class SettingInfo
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public SettingInfo(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
