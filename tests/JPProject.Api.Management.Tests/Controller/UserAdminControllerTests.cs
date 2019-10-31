﻿using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using JPProject.Api.Management.Tests.Fakers.UserFakers;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using ServiceStack;
using Xunit;

namespace JPProject.Api.Management.Tests.Controller
{
    public class UserAdminControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;

        public UserAdminControllerTests(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            _client = Factory.CreateClient();
        }
        private async Task Login()
        {
            _token = await Factory.IdentityServerClient.GetResourceOwnerPasswordAccessTokenAsync(
                new ClientConfiguration("TestClient", "MySecret"),
                new UserLoginConfiguration("user", "password"),
                "jp_api.user", "jp_api.is4");
            // The endpoint or route of the controller action.
            _client.SetBearerToken(_token.AccessToken);
        }


        [Fact]
        public async Task ShouldListUsers()
        {
            await Login();
            var newUser = UserViewModelFaker.GenerateUserViewModel().Generate();

            var response = await _client.PostAsync("/sign-up",
                new StringContent(newUser.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            var httpResponse = await _client.GetAsync("/admin/users");
            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var users = stringResponse.FromJson<ListOf<UserListViewModel>>();

            users.Should().NotBeNull();

        }
    }
}