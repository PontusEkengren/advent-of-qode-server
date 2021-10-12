using System;
using System.Collections.Generic;
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
        private DbContextOptions<AdventContext> _adventOptions;

        public When_creating_a_new_question_with_correct_input()
        {
            //Arrange
            _adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        }

        [Fact]
        public async void Should_return_successful()
        {
            //Act
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

            await using (var adventContext = new AdventContext(_adventOptions))
            {
                var queryController = new QueryController(adventContext);
                var response = await queryController.CreateQuestion(queryInput) as CreatedResult;

                //Assert
                response.StatusCode.Should().Be((int)HttpStatusCode.Created);
            }
        }
    }
}
