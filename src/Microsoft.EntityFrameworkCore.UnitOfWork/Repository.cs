﻿// Copyright (c) Arch team. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Represents a default generic repository implements the <see cref="IRepository{TEntity}"/> interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<TEntity>();
        }

        /// <summary>
        /// Changes the table name. This require the tables in the same database.
        /// </summary>
        /// <param name="table"></param>
        /// <remarks>
        /// This only been used for supporting multiple tables in the same model. This require the tables in the same database.
        /// </remarks>
        public void ChangeTable(string table)
        {
            if (_dbContext.Model.FindEntityType(typeof(TEntity)).Relational() is RelationalEntityTypeAnnotations relational)
            {
                relational.TableName = table;
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <returns>An <see cref="IQueryable{TEntity}"/> that contains elements that satisfy the condition specified by predicate.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null, bool disableTracking = true)
        {
            IQueryable<TEntity> set;
            if (disableTracking)
            {
                set = _dbSet.AsNoTracking();
            }
            else
            {
                set = _dbSet;
            }
            
            if (predicate != null)
            {
                set = set.Where(predicate);
            }

            return set;
        }

        /// <summary>
        /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public IPagedList<TEntity> GetPagedList(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int pageIndex = 0, int pageSize = 20, bool disableTracking = true, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToPagedList(pageIndex, pageSize);
            }
            else
            {
                return query.ToPagedList(pageIndex, pageSize);
            }
        }
        
        /// <summary>
        /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageIndex">The index of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        public Task<IPagedList<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageIndex = 0, int pageSize = 20, bool disableTracking = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
            else
            {
                return query.ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
            }
        }

        /// <summary>
        /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
        /// </summary>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
        public IQueryable<TEntity> FromSql(string sql, params object[] parameters) => _dbSet.FromSql(sql, parameters);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The found entity or null.</returns>
        public TEntity Find(params object[] keyValues) => _dbSet.Find(keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A <see cref="Task{TEntity}" /> that represents the asynchronous insert operation.</returns>
        public Task<TEntity> FindAsync(params object[] keyValues) => _dbSet.FindAsync(keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
        public Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken) => _dbSet.FindAsync(keyValues, cancellationToken);

        /// <summary>
        /// Inserts a new entity synchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public void Insert(TEntity entity)
        {
            var entry = _dbSet.Add(entity);
        }

        /// <summary>
        /// Inserts a range of entities synchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        public void Insert(params TEntity[] entities) => _dbSet.AddRange(entities);

        /// <summary>
        /// Inserts a range of entities synchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        public void Insert(IEnumerable<TEntity> entities) => _dbSet.AddRange(entities);

        /// <summary>
        /// Inserts a new entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous insert operation.</returns>
        public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dbSet.AddAsync(entity, cancellationToken);

            // Shadow properties?
            //var property = _dbContext.Entry(entity).Property("Created");
            //if (property != null) {
            //property.CurrentValue = DateTime.Now;
            //}
        }

        /// <summary>
        /// Inserts a range of entities asynchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous insert operation.</returns>
        public Task InsertAsync(params TEntity[] entities) => _dbSet.AddRangeAsync(entities);

        /// <summary>
        /// Inserts a range of entities asynchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous insert operation.</returns>
        public Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken)) => _dbSet.AddRangeAsync(entities, cancellationToken);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);

            // Shadow properties?
            //var property = _dbContext.Entry(entity).Property("LastUpdated");
            //if(property != null) {
            //property.CurrentValue = DateTime.Now;
            //}
        }

        /// <summary>
        /// Updates the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Update(params TEntity[] entities) => _dbSet.UpdateRange(entities);

        /// <summary>
        /// Updates the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Update(IEnumerable<TEntity> entities) => _dbSet.UpdateRange(entities);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(TEntity entity) => _dbSet.Remove(entity);

        /// <summary>
        /// Deletes the entity by the specified primary key.
        /// </summary>
        /// <param name="id">The primary key value.</param>
        public void Delete(object id)
        {
            // using a stub entity to mark for deletion
            var typeInfo = typeof(TEntity).GetTypeInfo();
            var key = _dbContext.Model.FindEntityType(typeInfo.Name).FindPrimaryKey().Properties.FirstOrDefault();
            var property = typeInfo.GetProperty(key?.Name);
            if (property != null)
            {
                var entity = Activator.CreateInstance<TEntity>();
                property.SetValue(entity, id);
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                var entity = _dbSet.Find(id);
                if (entity != null)
                {
                    Delete(entity);
                }
            }
        }

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Delete(params TEntity[] entities) => _dbSet.RemoveRange(entities);

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Delete(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);
    }
}
