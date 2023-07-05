﻿using FitnessApp.Data;
using FitnessApp.Services.Contracts;
using FitnessApp.Web.ViewModels.Models.Exercise;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Services
{
    public class ExerciseService : IExerciseService
    {

        public readonly ApplicationDbContext dbContext;
        public ExerciseService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ExerciseViewModel> AddToWorkout(int exerciseId, int workoutId)
        {
            var exercise = await dbContext.Exercises.FindAsync(exerciseId);
            var workout = await dbContext.Workouts.FindAsync(workoutId);

            if (exercise == null || workout == null)
            {
                throw new ArgumentException("Exercise or workout not found");
            }

            // Check if the exercise is already associated with the workout
            bool isAssociated = await dbContext.ExerciseWorkouts
                .AnyAsync(ew => ew.ExerciseId == exerciseId && ew.WorkoutId == workoutId);

            if (isAssociated)
            {
                throw new InvalidOperationException("Exercise is already associated with the workout");
            }

            // Create a new mapping entity
            ExerciseWorkout exerciseWorkout = new ExerciseWorkout
            {
                Exercise = exercise,
                Workout = workout
            };

            // Add the mapping entity to the context and save changes
            dbContext.ExerciseWorkouts.Add(exerciseWorkout);
            await dbContext.SaveChangesAsync();

            // Create the view model using the exercise details
            ExerciseViewModel exerciseViewModel = new ExerciseViewModel
            {
                Name = exercise.Name,
                Description = exercise.Description,
                Sets = exercise.Sets,
                Reps = exercise.Reps
            };

            return exerciseViewModel;
        }

        public Task CreateAsync(ExerciseViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ExerciseViewModel> GetAddModel()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ExerciseViewModel>> GetAll(int workoutId)
        {
            var mappedExerciseIds = await dbContext.ExerciseWorkouts
          .Where(we => we.WorkoutId == workoutId)
          .Select(we => we.ExerciseId)
          .ToListAsync();

            var model = await dbContext.Exercises
                .Where(e => !mappedExerciseIds.Contains(e.ExerciseId))
                .Select(m => new ExerciseViewModel
                {
                    Id = m.ExerciseId,
                    Name = m.Name,
                    Description = m.Description,
                    Reps = m.Reps,
                    Sets = m.Sets,
                }).ToListAsync();

            return model;
        }

        public async Task Remove(int ExerciseId)
        {
            var exercise = await dbContext.Exercises
                .FindAsync(ExerciseId);

            if (exercise != null)
            {
                dbContext.Exercises.Remove(exercise);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
