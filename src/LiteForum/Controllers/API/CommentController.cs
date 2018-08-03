using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteForum.Entities.Models;
using LiteForum.Helpers;
using LiteForum.Models;
using LiteForum.Services.Interfaces;
using LiteForum.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LiteForum.Controllers.API {
    [Authorize (policy: "Authenticated")]
    [Route ("api/post/{postId}/[controller]")]
    [ApiController]
    public class CommentController : BaseApiController {
        private readonly ILogger<CommentController> _logger;
        private readonly IDataService<LiteForumDbContext, Comment> _comments;

        public CommentController (ILogger<CommentController> logger,
            IDataService<LiteForumDbContext, Comment> comments) {
            _logger = logger;
            _comments = comments;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get (int postId) {
            try {
                var Comments = await _comments.GetAsync (filter: c => c.PostId == postId);
                return Ok (Comments.Select (c => c.ToVModel ()));
            } catch (Exception e) {
                _logger.LogError ($"Failed to fetch comments due to {e.Message ?? e.InnerException.Message}");
                return BadRequest (e.ToResponse (500));
            }
        }

        [HttpGet ("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSingle (int postId, int id, bool withChild = false) {
            if (id <= 0) throw new IndexOutOfRangeException ($"submitted id: {id} is not valid");

            Expression<Func<Comment, bool>> filter = c => c.PostId == postId && c.Id == id;
            try {
                var comment = withChild ?
                    await _comments.GetOneAsync (filter: filter, includeProperties: "Replies") :
                    await _comments.GetOneAsync (filter: filter);
                if (comment == null) return NotFound ();
                return Ok (comment?.ToVModel ());
            } catch (Exception e) {
                _logger.LogError ($"failed to get comment with id: {id}, due to {e.Message ?? e.InnerException.Message}");
                return BadRequest (e.ToResponse (500));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create (int postId, [FromBody] CommentVModel comment) {
            try {
                var newComment = comment.ToModel ();
                newComment.PostId = postId;
                newComment.UserId = UserId;
                newComment = _comments.Create (newComment, UserId);
                await _comments.SaveAsync ();
                _logger.LogInformation ($"User: {UserId} created a new comment {newComment}");
                return Created (Request.Path.Value, newComment.ToVModel ());
            } catch (Exception e) {
                _logger.LogError ($"comment creation by {UserId} failed due to {e.Message ?? e.InnerException.Message}");
                return BadRequest (e.ToResponse (500));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update (int postId, [FromBody] CommentVModel comment) {
            try {
                var oldComment = await _comments.GetByIdAsync (comment.Id);
                if (oldComment.UserId != UserId) throw new UnauthorizedAccessException (userMismatchMessage);
                if (oldComment.PostId != postId) throw new AccessViolationException (postMismatchMessage);
                oldComment.Content = comment.Content;
                _comments.Update (oldComment, UserId);
                await _comments.SaveAsync ();
                _logger.LogInformation ($"User: {UserId} modified a his comment {oldComment}");
                return Ok (oldComment.ToVModel ());
            } catch (Exception e) {
                _logger.LogError ($"comment modification by {UserId} failed due to {e.Message ?? e.InnerException.Message}");
                return BadRequest (e.ToResponse (500));
            }
        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> Delete (int postId, int id) {
            if (id <= 0) throw new IndexOutOfRangeException ("submitted id: ${id} is invalid");

            try {
                var comment = await _comments.GetByIdAsync (id);
                if (comment.UserId != UserId) throw new UnauthorizedAccessException (userMismatchMessage);
                if (comment.PostId != postId) throw new AccessViolationException (postMismatchMessage);

                _comments.Delete (id);
                await _comments.SaveAsync ();
                _logger.LogInformation ($"User: {UserId} deleted his comment with id: {id}");
                return Ok (new LiteForumResponseMessage (200, "deleted successfully"));
            } catch (Exception e) {
                _logger.LogError ($"comment deletion by {UserId} failed due to {e.Message ?? e.InnerException.Message}");
                return BadRequest (e.ToResponse (500));
            }
        }

        #region Helpers
        private const string userMismatchMessage = "comment was authored by another user.";
        private const string postMismatchMessage = "comment does not belong to the specified post.";
        #endregion
    }
}
