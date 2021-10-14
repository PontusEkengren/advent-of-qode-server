using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            _queryController = new QueryController(adventContext);
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

            var response = await _queryController.CreateQuestion(queryInput) as CreatedResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.Created);

            var getResponse = await _queryController.GetQuestion(1) as OkObjectResult;
            var question = getResponse.Value as QuestionViewModel;
            question.Question.Should().Be("Is Santa Red?");
            question.Options.Length.Should().Be(2);
        }
    }
}
