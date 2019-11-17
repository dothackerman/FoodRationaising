using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FoodRationaising.Controllers
{
    [Route("api/rationing")]
    [ApiController]
    public class RationingController : ControllerBase
    {
        [HttpPost("")]
        public ActionResult<List<DailyBucket>> UploadData([FromBody]List<FoodDto> foodDtos, [FromQuery]double dailyNeededCalories)
        {
            var data = foodDtos
                .GroupBy(x => x.FoodType)
                .ToDictionary(x => x.Key, y => y.Select(i => new Food { Name = i.Name, Calories = i.Calories }).ToList());

            var totalCalories = GetTotalCalories(data);

            var sweetsCaloriesAmount = dailyNeededCalories * 0.012;
            var meatAmount = dailyNeededCalories * 0.262;
            var veggiesAmount = dailyNeededCalories * 0.476;
            var carbohydratesAmount = dailyNeededCalories * 0.25;

            var dailyBuckets = new List<DailyBucket>();

            while (totalCalories > dailyBuckets.Sum(x => x.TotalCalories))
            {
                var dailyBucket = new DailyBucket();

                if (data[FoodType.Sweets].Any(x => x.Calories != 0.0D))
                {
                    var temp = GetFood(data[FoodType.Sweets], sweetsCaloriesAmount);
                    dailyBucket.Foods.AddRange(temp);
                }

                if (data[FoodType.Meat].Any(x => x.Calories != 0.0D))
                {
                    var missingAmount = sweetsCaloriesAmount - dailyBucket.TotalCalories;
                    dailyBucket.Foods.AddRange(GetFood(data[FoodType.Meat], missingAmount + meatAmount));
                }

                if (data[FoodType.Veggies].Any(x => x.Calories != 0.0D))
                {
                    var missingAmount = (sweetsCaloriesAmount + meatAmount) - dailyBucket.TotalCalories;
                    missingAmount += veggiesAmount;
                    dailyBucket.Foods.AddRange(GetFood(data[FoodType.Veggies], missingAmount));
                }

                if (data[FoodType.Carbs].Any(x => x.Calories != 0.0D))
                {
                    var missingAmount = (sweetsCaloriesAmount + meatAmount + veggiesAmount) - dailyBucket.TotalCalories;
                    missingAmount += carbohydratesAmount;
                    dailyBucket.Foods.AddRange(GetFood(data[FoodType.Carbs], missingAmount));
                }

                dailyBuckets.Add(dailyBucket);
            }

            return dailyBuckets;
        }

        private List<Food> GetFood(List<Food> foods, double calories)
        {
            var newFoods = new List<Food>();

            foreach (var food in foods)
            {
                if (food.Calories > calories)
                {
                    food.Calories -= calories;
                    newFoods.Add(new Food { Name = food.Name, Calories = calories });
                    break;
                }

                if (food.Calories == calories)
                {
                    newFoods.Add(new Food { Name = food.Name, Calories = calories });
                    food.Calories = 0;
                    break;
                }

                newFoods.Add(new Food { Name = food.Name, Calories = food.Calories });
                food.Calories = 0;
            }

            return newFoods;
        }

        private double GetTotalCalories(Dictionary<FoodType, List<Food>> data)
        {
            var totalCalories = 0D;

            foreach (var dataPoint in data)
            {
                totalCalories += dataPoint.Value.Sum(x => x.Calories);
            }

            return totalCalories;
        }
    }
}
