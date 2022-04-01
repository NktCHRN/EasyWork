using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BansController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public readonly IBanService _banService;

        private readonly IMapper _mapper;

        public BansController(IMapper mapper, UserManager<User> userManager, IBanService banService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _banService = banService;
        }
    }
}
