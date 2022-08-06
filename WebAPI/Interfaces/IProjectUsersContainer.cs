namespace WebAPI.Interfaces
{
    public interface IProjectUsersContainer
    {
        /// <summary>
        /// Adds a new connection id to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        /// <returns>Was new user (USER, not just the connection) added or not</returns>
        bool Add(int projectId, int userId, string connectionId);

        /// <summary>
        /// Deletes the connectionId from all the projects 
        /// and returns ids of projects from which the user was completely deleted 
        /// (they had no other connection ids on this project)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        /// <returns>IEnumerable of ids of projects from which the user was just deleted</returns>
        IEnumerable<int> Delete(int userId, string connectionId);

        /// <summary>
        /// Returns all users who are currently on the page of the project with the given id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>IEnumerable of ids of all users who are currently on the page of the chosen project</returns>
        IEnumerable<int> GetUsersOnProject(int projectId);
    }
}
