using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FoodRationaising
{
    public class FoodDto
    {
        public FoodType FoodType { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
    }

    public class Food
    {
        public string Name { get; set; }
        public double Calories { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FoodType
    {
        Carbs,
        Veggies,
        Meat,
        Sweets
    }

    public class DailyBucket
    {
        public List<Food> Foods { get; } = new List<Food>();
        [JsonIgnore]
        public double TotalCalories => Foods.Sum(x => x.Calories);
    }
}
