using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace OHContainer.Ratings
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string responseMessage;
            log.LogInformation("CreateRating function started.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string userId = data.userId;
            string productId = data.productId;
            string locationName = data.locationName;
            string rating = data.rating;
            string userNotes = data.userNotes;
/*
Validate both userId and productId by calling the existing API endpoints. You can find a user id to test with from the sample payload above
Add a property called id with a GUID value
Add a property called timestamp with the current UTC date time
Validate that the rating field is an integer from 0 to 5
Use a data service to store the ratings information to the backend
Return the entire review JSON payload with the newly created id and timestamp
*/
            // Populate new rating
            Rating resRating = new Rating();

            // Validate productId
            using (var httpClient = new HttpClient()) {
                var result = Task.Run(async () => await httpClient.GetAsync("https://serverlessohapi.azurewebsites.net/api/GetProduct?productId=" + productId)).Result;
                if (result.IsSuccessStatusCode) 
                    resRating.productId = productId;
                else {
//                    responseMessage = Task.Run(async () => await result.Content.ReadAsStringAsync()).Result;
                    return new BadRequestResult();
              }
            }

            // Validate userId
            using (var httpClient = new HttpClient()) {
                var result = Task.Run(async () => await httpClient.GetAsync("https://serverlessohapi.azurewebsites.net/api/GetUser?userId=" + userId)).Result;
                if (result.IsSuccessStatusCode) 
                    resRating.userId = userId;
                else {
//                    responseMessage = Task.Run(async () => await result.Content.ReadAsStringAsync()).Result;
                    return new BadRequestResult();
              }
            }

            // Validate user rating is 0..5
            int rat;
            if (!int.TryParse(rating, out rat) || (rat < 0) || (rat > 5)) {
                return new BadRequestResult();
            }
            resRating.rating = rat;

            // Finalize the resRating object
            resRating.id = Guid.NewGuid().ToString();
            resRating.timestamp = DateTime.UtcNow.ToString();
            resRating.locationName = locationName;
            resRating.userNotes = userNotes;


            // Store object in CosmosDB


            responseMessage = JsonConvert.SerializeObject(resRating);

            log.LogInformation("CreateRating function finished.");
            return new OkObjectResult(responseMessage);
        }
    }
}
