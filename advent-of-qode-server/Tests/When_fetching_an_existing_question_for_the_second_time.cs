using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Domain;
using advent_of_qode_server.Logic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class When_fetching_an_existing_question_for_the_second_time
    {
        private QueryController _queryController;
        AdventContext _adventContext;
        DateTime _expectedStartTime = DateTime.Parse("2022-11-22 19:51");
        string _admin1 = "admin@admin.se";
        public When_fetching_an_existing_question_for_the_second_time()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _adventContext = new AdventContext(adventOptions);
            _adventContext.Questions.Add(new Question { Day = 1, Query = "What color-theme is Santa?", Year = 2022, Options = new List<Option>() { new Option { Text = "Red", IsCorrectAnswer = true }, new Option { Text = "Blue" } } });
            _adventContext.StartTime.Add(new StartTime { Started = _expectedStartTime, Question = 1, UserEmail = _admin1 });
            _adventContext.SaveChanges();
            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"Uniqode:Admins",_admin1}
                })
                .Build();



            var fakeGoogleService = A.Fake<IGoogleService>();
            var fakeScoreService = A.Fake<IScoreService>();
            A.CallTo(() => fakeGoogleService.GetEmailByGmailToken(A<StringValues>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult(_admin1));

            _queryController = new QueryController(_adventContext, fakeConfig, fakeGoogleService, fakeScoreService);
        }

        [Fact]
        public async void Should_increase_query_counter()
        {
            await _queryController.GetQuestion(1, _admin1);

            _adventContext.StartTime.ToList().Should().HaveCount(1);
            var startTime = _adventContext.StartTime.Single();
            startTime.Started.Should().NotBe(DateTime.MinValue);
            startTime.UserEmail.Should().Be("admin@admin.se");
            startTime.Question.Should().Be(1);
            startTime.QuestionSeen.Should().Be(1);

            await _queryController.GetQuestion(1, _admin1);
            startTime.QuestionSeen.Should().Be(2);
        }
    }
}