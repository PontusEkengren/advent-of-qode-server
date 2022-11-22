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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
            _adventController = new AdventController(_adventContext);
        }

        [Fact]
        public async void Should_create_user_score_based_on_worst_time_possible() 
        {
            _adventController.CreateUserScore(new ScoreInputModel { Email = _admin1, Question = 1, Score= 1, UserId= _admin1 });

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
            var time = DateTime.Now;
            ScoreCalulator.Calculate(time, time).Should().Be(207);
        }

        [Fact]
        public async void Should_create_user_score_based_on_14_second_time()
        {
            var time = DateTime.Now;
            var startTime = time.AddSeconds(-1);
            ScoreCalulator.Calculate(startTime, time).Should().Be(197);
        }


        [Fact]
        public async void Should_create_user_score_based_on_10_second_time()
        {
            var time = DateTime.Now;
            var startTime = time.AddSeconds(-5);
            ScoreCalulator.Calculate(startTime, time).Should().Be(153);
        }


        [Fact]
        public async void Should_create_user_score_based_on_5_second_time()
        {
            var time = DateTime.Now;
            var startTime = time.AddSeconds(-10);
            ScoreCalulator.Calculate(startTime, time).Should().Be(98);
        }

        [Fact]
        public async void Should_create_user_score_based_on_no_time_left()
        {
            var time = DateTime.Now;
            var startTime = time.AddSeconds(-15);
            ScoreCalulator.Calculate(startTime, time).Should().Be(43);
        }
    }
}