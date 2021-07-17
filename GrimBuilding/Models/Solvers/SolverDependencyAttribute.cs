using System;

namespace GrimBuilding.Solvers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    sealed class SolverDependencyAttribute : Attribute
    {
        public Type Dependency { get; }
        public SolverDependencyAttribute(Type dependency) => Dependency = dependency;
    }
}
