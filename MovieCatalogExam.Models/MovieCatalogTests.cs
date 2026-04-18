using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using Exam_TheMovieCatalogSystem_2026QA.MovieCatalogExam.Models;

namespace Exam_TheMovieCatalogSystem_2026QA
{
    [TestFixture]
    public class MovieCatalogTests
    {
        private RestClient client;
        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000/api";
        private const string StaticToken = "";   // остави празно

        private const string LoginEmail = "didi123@example.com";
        private const string LoginPassword = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrEmpty(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                var tempClient = new RestClient(BaseUrl);

                var loginRequest = new RestRequest("/User/Authentication", Method.Post);
                loginRequest.AddJsonBody(new
                {
                    email = LoginEmail,
                    password = LoginPassword
                });

                var loginResponse = tempClient.Execute(loginRequest);

                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(loginResponse.Content);
                jwtToken = json["accessToken"].ToString();
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        // 1.3 CREATE
        [Order(1)]
        [Test]
        public void CreateNewMovie_WithRequiredFields()
        {
            var request = new RestRequest("/Movie/Create", Method.Post);

            request.AddJsonBody(new
            {
                Title = "Test Movie",
                Description = "This is a Test Description",
                PosterUrl = "https://example.com/poster.jpg",
                TrailerLink = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                IsWatched = true
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Assert.That(result.movie, Is.Not.Null);
            Assert.That(result.movie.Id, Is.Not.Null.And.Not.Empty);

            createdMovieId = result.movie.Id;
        }

        // 1.4 EDIT
        [Order(2)]
        [Test]
        public void EditMovie_WithExistingId()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);

            request.AddJsonBody(new
            {
                Title = "Edited Movie",
                Description = "Edited Description"
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Assert.That(result.msg, Is.EqualTo("Movie edited successfully!"));
        }

        // 1.5 GET ALL MOVIES — правилният endpoint е /Catalog/All
        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnNonEmptyList()
        {
            var request = new RestRequest("/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            string content = response.Content.Trim();

            // Ако API-то върне масив []
            if (content.StartsWith("["))
            {
                var movies = JsonSerializer.Deserialize<List<MovieDTO>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Assert.That(movies, Is.Not.Null);
                Assert.That(movies.Count, Is.GreaterThan(0));
            }
            else
            {
                // Ако API-то върне ApiResponseDTO
                var result = JsonSerializer.Deserialize<ApiResponseDTO>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Assert.That(result.movies, Is.Not.Null);
                Assert.That(result.movies.Count, Is.GreaterThan(0));
            }
        }

        // 1.6 DELETE
        [Order(4)]
        [Test]
        public void DeleteMovie_WithExistingId()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Assert.That(result.msg, Is.EqualTo("Movie deleted successfully!"));
        }

        // 1.7 INVALID CREATE
        [Order(5)]
        [Test]
        public void CreateMovie_MissingRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Create", Method.Post);

            request.AddJsonBody(new
            {
                Title = "",
                Description = ""
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // 1.8 INVALID EDIT
        [Order(6)]
        [Test]
        public void EditMovie_NonExistingId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "non-existing-id-123");

            request.AddJsonBody(new
            {
                Title = "Doesn't matter",
                Description = "Doesn't matter"
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // 1.9 INVALID DELETE
        [Order(7)]
        [Test]
        public void DeleteMovie_NonExistingId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "non-existing-id-123");

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            client?.Dispose();
        }
    }
}
