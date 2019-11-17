using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FoodRationaising.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        [HttpPost]
        public ActionResult<List<DailyBucket>> UploadData([FromBody]List<FoodDto> foodDtos)
        {
            var data = foodDtos
                .GroupBy(x => x.FoodType)
                .ToDictionary(x => x.Key,y => y.ToList().Select(i => new Food { Name = i.Name, Calories = i.Calories }).ToList());

            var totalCalories = GetTotalCalories(data);
            var dailyNeededCalories = 2000D; //TODO calculate from data in request

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
                    var temp = GetFood(data[FoodType.Meat], missingAmount + meatAmount);
                    dailyBucket.Foods.AddRange(temp);
                }

                if (data[FoodType.Veggies].Any(x => x.Calories != 0.0D))
                {
                    var missingAmount = (sweetsCaloriesAmount + meatAmount) - dailyBucket.TotalCalories;
                    missingAmount += veggiesAmount;
                    var temp = GetFood(data[FoodType.Veggies], missingAmount);
                    dailyBucket.Foods.AddRange(temp);
                }

                if (data[FoodType.Carbs].Any(x => x.Calories != 0.0D))
                {
                    var missingAmount = (sweetsCaloriesAmount + meatAmount + veggiesAmount) - dailyBucket.TotalCalories;
                    missingAmount += carbohydratesAmount;
                    var temp = GetFood(data[FoodType.Carbs], missingAmount);
                    dailyBucket.Foods.AddRange(temp);
                }

                dailyBuckets.Add(dailyBucket);
            }

            return dailyBuckets;
        }

        private List<Food> GetFood(List<Food> foods, double calories)
        {
            var newFoods = new List<Food>();

            for (var index = 0; index < foods.Count; index++)
            {
                if (foods[index].Calories > calories)
                {
                    foods[index].Calories -= calories;
                    newFoods.Add(new Food { Name = foods[index].Name, Calories = calories });
                    break;
                }

                if (foods[index].Calories == calories)
                {
                    newFoods.Add(new Food { Name = foods[index].Name, Calories = calories });
                    foods[index].Calories = 0;
                    break;
                }

                newFoods.Add(new Food { Name = foods[index].Name, Calories = foods[index].Calories });
                foods[index].Calories = 0;
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
