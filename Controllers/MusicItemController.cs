using AppB2C2.Data;
using AppB2C2.Models.Domain;
using AppB2C2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceCalculatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicItemController : ControllerBase
    {
        private readonly DjDbContext _djDbContext;

        public MusicItemController(DjDbContext djDbContext)
        {
            _djDbContext = djDbContext;
        }

        // GET: api/MusicItem
        [SwaggerOperation(Summary = "Get all music items", Description = "Returns a list of all music items.")]
        [SwaggerResponse(200, "Successful operation", Type = typeof(IEnumerable<MusicItem>))]
        [HttpGet]
        public IActionResult Get()
        {
            var musicItems = _djDbContext.MusicItems.ToList();
            return Ok(musicItems);
        }

        // GET api/MusicItem/5
        [SwaggerOperation(Summary = "Get a music item by ID", Description = "Returns details of a specific music item.")]
        [SwaggerResponse(200, "Successful operation", Type = typeof(MusicItem))]
        [SwaggerResponse(404, "Music item not found")]
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var musicItem = _djDbContext.MusicItems
                .Include(item => item.ItemTags)
                .FirstOrDefault(item => item.Id == id);

            return Ok(musicItem);
        }

    

    // POST api/MusicItem
    [SwaggerOperation(Summary = "Add a new music item", Description = "Adds a new music item to the database.")]
        [SwaggerResponse(201, "Music item created", Type = typeof(MusicItem))]
        [HttpPost]
        public IActionResult Post([FromBody] AddMusicItemRequest addMusicItemRequest)
        {
            var musicItem = new MusicItem
            {
                ItemTitle = addMusicItemRequest.ItemTitle,
                ItemDescription = addMusicItemRequest.ItemDescription,
                Artist = addMusicItemRequest.Artist,
                ItemContent = addMusicItemRequest.ItemContent,
                DateAdded = addMusicItemRequest.DateAdded,
                ItemValue = addMusicItemRequest.ItemValue,
                ItemType = addMusicItemRequest.ItemType
            };

            // Logic for calculating TagPrice base on ItemType
            var tagPriceFactor = GetTagPriceFactor(addMusicItemRequest.ItemType);
            musicItem.ItemTags = _djDbContext.ItemTags.Where(tag => addMusicItemRequest.TagIds.Contains(tag.Id)).ToList();

            foreach (var tag in musicItem.ItemTags)
            {
                tag.TagPrice *= tagPriceFactor;
            }

            _djDbContext.MusicItems.Add(musicItem);
            _djDbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = musicItem.Id }, musicItem);
        }

        // PUT api/MusicItem/5
        [SwaggerOperation(Summary = "Update a music item", Description = "Updates details of a specific music item.")]
        [SwaggerResponse(200, "Successful operation", Type = typeof(MusicItem))]
        [SwaggerResponse(404, "Music item not found")]
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] EditMusicViewModel editViewModel)
        {
            var musicItem = _djDbContext.MusicItems
                .Include(item => item.ItemTags)
                .FirstOrDefault(item => item.Id == id);

            if (musicItem == null)
            {
                return NotFound();
            }

            musicItem.ItemTitle = editViewModel.ItemTitle;
            musicItem.ItemDescription = editViewModel.ItemDescription;
            musicItem.Artist = editViewModel.Artist;
            musicItem.DateAdded = editViewModel.DateAdded;
            musicItem.ItemValue = editViewModel.ItemValue;

            _djDbContext.SaveChanges();

            return Ok(musicItem);
        }

        // DELETE api/MusicItem/5
        [SwaggerOperation(Summary = "Delete a music item by ID", Description = "Deletes a specific music item.")]
        [SwaggerResponse(204, "Music item deleted")]
        [SwaggerResponse(404, "Music item not found")]
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var musicItem = _djDbContext.MusicItems.Find(id);

            if (musicItem == null)
            {
                return NotFound();
            }

            _djDbContext.MusicItems.Remove(musicItem);
            _djDbContext.SaveChanges();

            return NoContent();
        }

        private float GetTagPriceFactor(MusicItemType itemType)
        {
            switch (itemType)
            {
                case MusicItemType.CD:
                    return 0.7f;
                case MusicItemType.Cassette:
                    return 0.5f;
                case MusicItemType.LP:
                    return 1.2f;
                default:
                    return 1.0f;
            }
        }
    }
}
