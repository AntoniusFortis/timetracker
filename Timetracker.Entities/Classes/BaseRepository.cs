using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Timetracker.Entities.Classes
{
    public class BaseRepository<T> : IDisposable where T : class
    {
        protected TimetrackerContext _contextDB { get; set; }

        public virtual DbSet<T> Context => null;

        public async void Add(T T)
        {
            await Context.AddAsync(T);
        }

        public IQueryable<T> GetAll()
        {
            return Context.AsNoTracking();
        }

        public async void SaveAll()
        {
            await _contextDB.SaveChangesAsync(true);
        }

        public void Dispose()
        {
            _contextDB.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
