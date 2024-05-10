using System.Collections.Generic;

namespace Terminal_7.Classes
{
    internal class ConfigDeserializer
    {
        public int HackAttempts { get; set; } = 10;
        public bool CanBeHacked { get; set; } = true;
        public bool CanBeChanged { get; set; } = false;
        public bool CanBeCopied { get; set; } = false;
        public bool CanBeDeleted { get; set; } = false;
        public bool HasPassword { get; set; } = false;
        public Dictionary<string, string> LoginsAndPasswords { get; set; } = new Dictionary<string, string>();
    }
}
