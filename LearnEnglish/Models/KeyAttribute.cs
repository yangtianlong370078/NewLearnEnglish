namespace LearnEnglish.Models
{
    /// <summary>
    /// 属性标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(string keyName)
        {
            KeyName = keyName;
        }
        public string KeyName { get; }
    }

    public class IsEffectiveAttribute : Attribute
    {
        public IsEffectiveAttribute(bool isEffectiveIs)
        {
            IsEffectiveIs = isEffectiveIs;
        }
        public bool IsEffectiveIs { get; }
    }
}
