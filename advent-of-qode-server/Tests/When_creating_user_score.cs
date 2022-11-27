using System;
using System.Linq;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Logic;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests
{
    public class When_creating_user_score
    {
        private AdventController _adventController;
        private string _admin1 = "admin@admin.se";
        private AdventContext _adventContext;
        public When_creating_user_score()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _adventContext = new AdventContext(adventOptions);
            _adventContext.StartTime.Add(new StartTime { Started = DateTime.Parse("2022-11-22 00:00"), Question = 1, UserEmail = _admin1 });
            _adventContext.SaveChanges();

            _adventContext = new AdventContext(adventOptions);
            _adventController = new AdventController(_adventContext, new ScoreService(_adventContext));
        }

        [Fact]
        public async void Should_create_user_score_based_on_worst_time_possible()
        {
            _adventController.CreateUserScore(new ScoreInputModel { Email = _admin1, Question = 1, Score = 1, UserId = _admin1 });

            _adventContext.ScoreBoard.Should().HaveCount(1);
            var score = _adventContext.ScoreBoard.ToList().Single();
            score.UserEmail.Should().Be(_admin1);
            score.UserId.Should().Be(_admin1);
            score.Score.Should().Be(42);
            score.Question.Should().Be(1);
        }

        [Fact]
        public async void Should_create_user_score_based_on_best_time_possible()
        {
            var time = DateTime.UtcNow;
            ScoreCalulator.Calculate(time, time).Should().Be(206);
        }

        [Fact]
        public async void Should_create_user_score_based_on_14_second_time()
        {
            var time = DateTime.UtcNow;
            var startTime = time.AddSeconds(-1);
            ScoreCalulator.Calculate(startTime, time).Should().Be(196);
        }


        [Fact]
        public async void Should_create_user_score_based_on_10_second_time()
        {
            var time = DateTime.UtcNow;
            var startTime = time.AddSeconds(-5);
            ScoreCalulator.Calculate(startTime, time).Should().Be(152);
        }


        [Fact]
        public async void Should_create_user_score_based_on_5_second_time()
        {
            var time = DateTime.UtcNow;
            var startTime = time.AddSeconds(-10);
            ScoreCalulator.Calculate(startTime, time).Should().Be(97);
        }

        [Fact]
        public async void Should_create_user_score_based_on_no_time_left()
        {
            var time = DateTime.UtcNow;
            var startTime = time.AddSeconds(-15);
            ScoreCalulator.Calculate(startTime, time).Should().Be(42);
        }
    }
}