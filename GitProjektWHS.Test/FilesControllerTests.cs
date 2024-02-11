using GitProjektWHS.Models;
using FakeItEasy;
using System;
using Commons.Models;
using GitProjektWHS.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GitProjektWHS.Test
{

    public class FilesControllerTests
    {
        private VersionsContext _versionsContext;
        public FilesControllerTests()
        {
            var options = new DbContextOptionsBuilder<VersionsContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            _versionsContext = new VersionsContext(options);

            FillMockData(_versionsContext);
        }

        private void FillMockData(VersionsContext db)
        {
            db.Dateien.Add(new FileObject()
            {
                Id = 1,
                Locked = false,
                VersionIds = new List<long>() { 1, 2, 3 },
            });
        }

        [Fact]
        public async Task GetFile_ReturnsFound_WhenFileFound()
        {
         
            var controller = new FilesController(_versionsContext);

            // Act
            var result = await controller.GetFile(1);

            // Assert
            result.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task GetFile_ReturnsNotFound_WhenFileNotFound()
        {

            var controller = new FilesController(_versionsContext);

            // Act
            var result = await controller.GetFile(345);

            // Assert
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task PostFile_Unlocked()
        {
            var datei = new FileObject { Id = 9949, Locked= false, VersionIds = new List<long>() { 7, 8, 5 }, };
            var controller = new FilesController(_versionsContext);

            // Act
            var result = await controller.PostFile(datei);

            // Assert
            result.Result.Should().BeOfType(typeof(CreatedAtActionResult));
            result.Result.Should().NotBeNull();
        }

    }
}