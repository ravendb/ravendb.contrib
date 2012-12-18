using System;

namespace Raven.Client.Contrib.Tests.TestEntities
{
    public class Ball
    {
        public string Id { get; set; }
        public string Color { get; set; }

        public static string[] Colors = new[]
            {
                "red", "orange", "yellow", "green", "blue", "indigo",
                "violet", "brown", "black", "white", "silver", "gold"
            };

        private static readonly Random RandomNumber = new Random();
        public static Ball Random
        {
            get
            {
                var color = Colors[RandomNumber.Next(Colors.Length)];
                return new Ball { Color = color };
            }
        }
    }
}
