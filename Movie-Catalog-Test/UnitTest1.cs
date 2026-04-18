using Movie_Catalog_Test.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace Movie_Catalog_Test
{
    public class MovieCatalogTests
    {
        private RestClient client;
        private static string movieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("ViktorK@test.com", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = authClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
              
                var content = JsonConvert.DeserializeObject<JObject>(response.Content);
                var token = content["accessToken"]?.ToString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Failed to authenticate. Status code: {response.StatusCode}. Body: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewMovie_WithTheRequiredFields_ShouldSucceed()
        {
            // Arrange
            MovieDTO newMovie = new MovieDTO
            {
                Title = "Inception",
                Description = "A mind-bending thriller about dream invasion."
            };
            // Act
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            RestResponse response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            string? responseContent = response.Content;
            var readyResponse = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Movie, Is.InstanceOf<MovieDTO>());
            Assert.That(readyResponse!.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));

            movieId = readyResponse.Movie.Id;
            
        }

        [Order(2)]
        [Test]
        public void EditMovie_WithTheRequiredFields_ShouldSucceed()
        {
            {
              
                // Arrange&Act
                RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
                request.AddQueryParameter("movieId", movieId);
                request.AddJsonBody(new
                {

                    title = "Inception Edited",
                    description = "An edited description of the mind-bending thriller.",

                });

                RestResponse response = this.client.Execute(request);
                
                // Assert 
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var apiResponse = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);
                Assert.That(apiResponse.Msg, Is.EqualTo("Movie edited successfully!"));
            }

        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldSucceed()
        {
            // Arrange&Act
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = this.client.Execute(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var movies = JsonConvert.DeserializeObject<List<MovieDTO>>(response.Content);
            Assert.That(movies, Is.Not.Null);
            
        }

        [Order(4)]
        [Test]
        public void DeleteMovie_ShouldSucceed()
        {
            // Arrange&Act
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", movieId);
            RestResponse response = this.client.Execute(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var apiResponse = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldFail()
        {
            // Arrange
            MovieDTO newMovie = new MovieDTO
            {
                Description = "A movie without a title."
            };
            // Act
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            RestResponse response = this.client.Execute(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistentMovie_ShouldFail()
        {
            // Arrange
            string nonExistentMovieId = "2132452465";
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistentMovieId);
            request.AddJsonBody(new
            {
                title = "Non-existent Movie",
                description = "Trying to edit a movie that doesn't exist."
            });
            // Act
            RestResponse response = this.client.Execute(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var apiResponse = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistentMovie_ShouldFail()
        {
            // Arrange
            string nonExistentMovieId = "2132452465";
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistentMovieId);
            // Act
            RestResponse response = this.client.Execute(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var apiResponse = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }



        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }

    }
}