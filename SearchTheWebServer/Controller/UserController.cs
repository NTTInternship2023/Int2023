using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchTheWebServer.Data;
using SearchTheWebServer.Validators;
using SearchTheWebServer.Models;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace SearchTheWebServer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class UserController : ControllerBase
    {
        private string ApiKey { get; } = "c3eb1fafacmshc5fd818a156e962p1af460jsnc0f24ca50442";
        private string ApiHost { get; } = "moviesdatabase.p.rapidapi.com";

        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody]RegisterUserDto userDto)
        {
            try
            {
                //Validation block
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

                if (existingUser != null)
                {
                    return Conflict("Username already exists");
                }

                if (!PasswordValidator.PasswordPatternValidation(userDto.Password)) return Conflict("Invalid format for Password");
                if (!EmailValidator.EmailPatternValidation(userDto.Email)) return Conflict("Invalid format for Email");

                //Crypting the password
                CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                User user = new User();
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                //Creating the entity for the db
                User newUser = new User { Username = userDto.Username, Email = userDto.Email, PasswordHash = user.PasswordHash, PasswordSalt = user.PasswordSalt };

                //Adding and saving the entity for the db
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok($"New user created! Welcome, {newUser.Username}!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error registering user: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginStatus>> Login([FromBody]LoginUserDto userDto)
        {
            LoginStatus loginStatus = new LoginStatus();
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

                if (existingUser == null)
                {
                    loginStatus.Status= false;
                    loginStatus.Message= "Username unregistered, please register";
                    return loginStatus;
                }

                else if (!VerifyPasswordHash(userDto.Password, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    loginStatus.Status= false;
                    loginStatus.Message= "Wrong password";
                    return loginStatus;
                }
                loginStatus.Id = existingUser.Id;
                loginStatus.Username = existingUser.Username;
                loginStatus.Status= true;
                loginStatus.Message= "Succesful Login";
                return loginStatus;
                
            }
            catch (Exception ex) {
                loginStatus.Status= false;
                loginStatus.Message= $"Error logging in: {ex}";
                return loginStatus;
            }
        }
        [HttpPost("changepassword")]
        public async Task<ActionResult<User>> ChangePassword(NewPasswordUserDto userDto)
        {
            try
            {
                //validation block
                var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
                if (updatedUser == null)
                {
                    return BadRequest("User not found");
                }
                else if (!VerifyPasswordHash(userDto.Password, updatedUser.PasswordHash, updatedUser.PasswordSalt))
                {
                    return Conflict("Wrong password");
                }
                if (!PasswordValidator.PasswordPatternValidation(userDto.NewPassword)) return Conflict("Invalid format for Password");

                CreatePasswordHash(userDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                updatedUser.PasswordHash = passwordHash;
                updatedUser.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error changing password in: {ex.Message}");
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes((string)password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }


        /*
         * Function to get movie's rating from the API. It makes a GET request to the API and returns the rating.
         * Parameters: string movieId
         * Returns: double
         */
        private async Task<(double,int)> GetMovieRating(string movieId)
        {
            double rating = 0;
            int numVotes = 0;
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://moviesdatabase.p.rapidapi.com/titles/{movieId}/ratings"),
                Headers =
            {
                { "X-RapidAPI-Key", "70158d7bdcmsh70b948f4980dad3p1269b9jsn557b43f311ed" },
                { "X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com" }
            }
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                var results = root.GetProperty("results");

                if (results.ValueKind != JsonValueKind.Null)
                {
                    var ratingElement = results.GetProperty("averageRating");
                    rating = results.GetProperty("averageRating").GetDouble();
                    numVotes = results.GetProperty("numVotes").GetInt32();  
                }
            }

            return (rating,numVotes);
        }

        /*
        * Function to get data from the API. It makes a GET request to the API and returns DTOs.
         * Adds the search to the database.
         * Parameters: string title, int userId
         * Returns: Dto
         * Throws: Exception if the API call fails
        */
        [HttpGet]
        public async Task<ActionResult<List<MovieDto>>> GetDataFromApi(string title, int userId)
        {
            //Request to the API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://moviesdatabase.p.rapidapi.com/titles/search/title/{title}");
            request.Headers.Add("X-RapidAPI-Host", ApiHost);
            request.Headers.Add("X-RapidAPI-Key", ApiKey);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("false"), "exact");
            content.Add(new StringContent("movie,tvSeries"), "titleType");
            request.Content = content;

            var movieDtos = new List<MovieDto>();
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                var results = root.GetProperty("results");
                var resultList = results.EnumerateArray().ToList();

                foreach (var element in resultList)
                {
                    var titleString = element.GetProperty("titleText").GetProperty("text").ToString();
                    var imageElement = element.GetProperty("primaryImage");
                    var idString = element.GetProperty("id").ToString();
                    double rating;

                    var imageString = imageElement.ValueKind == JsonValueKind.Null
                        ? "https://reprospecialty.com/wp-content/themes/apexclinic/images/no-image/No-Image-Found-400x264.png"
                        : imageElement.GetProperty("url").ToString();
                    var releaseYear = element.GetProperty("releaseYear").GetProperty("year").GetInt32();

                    var ratingElement = await GetMovieRating(idString);

                    movieDtos.Add(new MovieDto
                    {
                        Id = idString,
                        Title = titleString,
                        ImageUrl = imageString,
                        ReleaseYear = releaseYear,
                        Rating = ratingElement.Item1,
                        NumVotes=ratingElement.Item2
                    });
                }


                var searchLog = new SearchLog
                {
                    IdUser = userId,
                    Date = DateTime.Now,
                    Action = "search",
                    ActionDetail = title
                };

                _context.SearchLogs.Add(searchLog);
                await _context.SaveChangesAsync();


                return Ok(movieDtos);
            }
        }
        [HttpPost("Filter")]
        public async Task<ActionResult<List<MovieDto>>> FilterMovies([FromBody]FilterDTO filterDTO)
        {
            
            //Request to the API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://moviesdatabase.p.rapidapi.com/titles/search/title/{filterDTO.Title}");
            request.Headers.Add("X-RapidAPI-Host", ApiHost);
            request.Headers.Add("X-RapidAPI-Key", ApiKey);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("false"), "exact");
            content.Add(new StringContent("20"), "limit");
            content.Add(new StringContent(filterDTO.TitleType??"movie,tvSeries"), "titleType");

            //Validating input
            FilterParametersValidator validator = new FilterParametersValidator();
           if (filterDTO.sort != null && !validator.IsValidSort(filterDTO.sort))
            {
                return Conflict("Sort parameter is Invalid, only incr or decr available");
            }

            if (!(validator.IsValidYear(filterDTO.EndYear) && validator.IsValidYear(filterDTO.StartYear)))
            {
                return Conflict("Invalid time parameters");
            }
            if (!(validator.IsValidTimeInterval(filterDTO.StartYear, filterDTO.EndYear)))
            {
                return Conflict("Invalid time interval");
            }

            request.Content = content;

            var movieDtos = new List<MovieDto>();
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                var results = root.GetProperty("results");
                var resultList = results.EnumerateArray().ToList();

                if(resultList.Count == 0) {
                    return Conflict("No results found");
                }
                foreach (var element in resultList)
                {
                    var titleString = element.GetProperty("titleText").GetProperty("text").ToString();
                    var imageElement = element.GetProperty("primaryImage");
                    var idString = element.GetProperty("id").ToString();
                    double rating=0;

                    var imageString = imageElement.ValueKind == JsonValueKind.Null
                        ? "https://reprospecialty.com/wp-content/themes/apexclinic/images/no-image/No-Image-Found-400x264.png"
                        : imageElement.GetProperty("url").ToString();
                    var releaseYear = element.GetProperty("releaseYear").GetProperty("year").GetInt32();

                    var ratingElement = await GetMovieRating(idString);

                    movieDtos.Add(new MovieDto
                    {
                        Id = idString,
                        Title = titleString,
                        ImageUrl = imageString,
                        ReleaseYear = releaseYear,
                        Rating = ratingElement.Item1,
                        NumVotes=ratingElement.Item2
                    });
                }

                if (filterDTO.StartYear != null && filterDTO.EndYear == null)
                {
                   movieDtos= movieDtos.FindAll(m => m.ReleaseYear >= filterDTO.StartYear);
                }
                else if (filterDTO.StartYear == null && filterDTO.EndYear != null)
                {
                    movieDtos = movieDtos.FindAll(m => m.ReleaseYear <= filterDTO.EndYear );
                }
                else if( filterDTO.StartYear != null && filterDTO.EndYear != null )
                {
                    movieDtos = movieDtos.FindAll(m => (m.ReleaseYear <= filterDTO.EndYear  && m.ReleaseYear >= filterDTO.StartYear));
                }
          
var searchLog = new SearchLog
{
    IdUser = filterDTO?.IdUser ?? default,
    Date = DateTime.Now,
    Action = "search",
    ActionDetail = filterDTO?.Title ?? string.Empty
};





                _context.SearchLogs.Add(searchLog);
                await _context.SaveChangesAsync();
if (string.Equals(filterDTO?.sort, "incr", StringComparison.OrdinalIgnoreCase))
{
    return Ok(movieDtos.OrderBy(m => m.ReleaseYear));
}
else
{
    return Ok(movieDtos.OrderByDescending(m => m.ReleaseYear));
}






             
            }

        }

        /*
 * Function to get movie's award details from the API. It makes a GET request to the API and returns the award details.
 * Parameters: string titleId
 * Returns: AwardDetails
 */
private async Task<AwardDetailsDto> GetMovieAwardDetails(string titleId)
{
    AwardDetailsDto awardDetails = new AwardDetailsDto();
    var client = new HttpClient();
    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Get,
        RequestUri = new Uri($"https://moviesdatabase.p.rapidapi.com/titles/{titleId}?info=awards"),
        Headers =
        {
            { "X-RapidAPI-Key", ApiKey },
            { "X-RapidAPI-Host", ApiHost }
        }
    };

    using (var response = await client.SendAsync(request))
    {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(body);
        
        var root = document.RootElement.GetProperty("results");

        awardDetails.NumberOfNominations = root.GetProperty("nominations").GetProperty("total").GetInt32();
        awardDetails.NumberOfWins = root.GetProperty("wins").GetProperty("total").GetInt32();
    }
    

    return awardDetails;
}

/*
 * HTTP Endpoint to get movie's award details from the API.
 * Parameters: string titleId
 * Returns: AwardDetails
 */
[HttpGet("awards/{titleId}")]
public async Task<ActionResult<AwardDetailsDto>> GetAwards(string titleId)
{
    try
    {
        var awardDetails = await GetMovieAwardDetails(titleId);
        return Ok(awardDetails);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error getting award details: {ex.Message}");
    }
}

    }
}