using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using FakeItEasy;
using FluentAssertions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class When_creating_a_new_question_with_correct_input
    {
        private QueryController _queryController;

        public When_creating_a_new_question_with_correct_input()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adventContext = new AdventContext(adventOptions);
            var admin_1 = "admin@admin.se";
            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"Uniqode:Admins",admin_1}
                })
                .Build();

            var fakeGoogleService = A.Fake<IGoogleService>();
            A.CallTo(() => fakeGoogleService.GetEmailByGmailToken(A<StringValues>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult(admin_1));

            _queryController = new QueryController(adventContext, fakeConfig, fakeGoogleService);
        }

        [Fact]
        public async void Should_return_successful()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 1,
                Question = "Is Santa Red?",
                Options = new List<OptionInputModel>
                {
                    new() { Text = "No, fabricated by CocaCola!", IsCorrectAnswer = false },
                    new() { Text = "Certainly", IsCorrectAnswer = true }
                }
            };

            var response = await _queryController.AddOrUpdateQuestion(queryInput) as OkObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var getResponse = await _queryController.GetQuestion(1) as OkObjectResult;
            var question = getResponse.Value as QuestionViewModel;
            question.Question.Should().Be("Is Santa Red?");
            question.Options.Length.Should().Be(2);
        }
    }
}
