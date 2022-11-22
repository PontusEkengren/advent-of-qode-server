using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Domain;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;
using static IdentityServer4.Models.IdentityResources;

namespace Tests
{
    public class When_fetching_an_existing_question
    {
        private QueryController _queryController;
        AdventContext _adventContext;
        DateTime _expectedStartTime = DateTime.Parse("2022-11-22 19:51");
        public When_fetching_an_existing_question()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var admin_1 = "admin@admin.se";

            _adventContext = new AdventContext(adventOptions);
            _adventContext.Questions.Add(new Question { Day = 1, Query = "What color-theme is Santa?", Year = 2022, Options = new List<Option>() { new Option { Text = "Red", IsCorrectAnswer = true }, new Option { Text = "Blue" } } });
            _adventContext.StartTime.Add(new StartTime { Started = _expectedStartTime, Question = 1, UserEmail = admin_1 });
            _adventContext.SaveChanges();
            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"Uniqode:Admins",admin_1}
                })
                .Build();

            var fakeGoogleService = A.Fake<IGoogleService>();
            A.CallTo(() => fakeGoogleService.GetEmailByGmailToken(A<StringValues>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult(admin_1));

            _queryController = new QueryController(_adventContext, fakeConfig, fakeGoogleService);
        }

        [Fact]
        public async void Should_return_question_for_requested_day()
        {
            var response = await _queryController.GetQuestion(1) as OkObjectResult;

            var getResponse = await _queryController.GetQuestion(1) as OkObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var question = getResponse.Value as QuestionViewModel;
            question.Question.Should().Be("What color-theme is Santa?");
            question.Options.Length.Should().Be(2);
        }

        [Fact]
        public async void Should_create_a_start_time_for_counting_points()
        {
            await _queryController.GetQuestion(1);

            _adventContext.StartTime.ToList().Should().HaveCount(1);
            var startTime = _adventContext.StartTime.Single();
            startTime.Started.Should().NotBe(DateTime.MinValue);
            startTime.UserEmail.Should().Be("admin@admin.se");
            startTime.Question.Should().Be(1);
        }

        [Fact]
        public async void Should_not_update_start_time_when_a_start_time_already_exists()
        {
            await _queryController.GetQuestion(1);

            _adventContext.StartTime.ToList().Should().HaveCount(1);
            var startTime = _adventContext.StartTime.Single();
            startTime.Started.Should().Be(_expectedStartTime);
        }
    }
}