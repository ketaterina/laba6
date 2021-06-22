using System.Runtime.Serialization;

namespace App
{
    [DataContract]

    class Student
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }


        public Student (string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}
