using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PsihoKorak_Platforma.Controllers;
using PsihoKorak_Platforma.Models;

namespace PsihoKorak_Platforma.Tests.Controller
{
    public class PsychologistControllerTests
    {
        private readonly PsihoKorakPlatformaContext _context;
        private readonly PsychologistController _controller;

        public PsychologistControllerTests()
        {
            _context = A.Fake<PsihoKorakPlatformaContext>();
            _controller = new PsychologistController(_context);
        }

        [Fact]
        public void PsychologistController_Login_ReturnsViewResult()
        {
            var result = _controller.Login();

            result.Should().BeOfType<ViewResult>();
        }

    }
}
