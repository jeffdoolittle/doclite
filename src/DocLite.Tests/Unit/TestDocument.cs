using System;

namespace DocLite.Tests.Unit
{
    public class TestDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private string _description;
        public string Description { get { return _description; } set { _description = value; } }
    }

    public class DocumentWithGuid
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public void SetName(string name)
        {
            Name = name;
        }

        private string _someValue;

        public string SomeOtherProperty { get { return _someValue; } set { _someValue = value; } }
    }

    public class SingletonDocument
    {
        public string Name { get; private set; }

        public void SetName(string value)
        {
            Name = value;
        }
    }
}