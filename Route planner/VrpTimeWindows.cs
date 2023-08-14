using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.ConstraintSolver;

namespace Route_planner
{
    public class VrpTimeWindows
    {
        // класс для поиска оптимального маршрута
        public class DataModel
        {
            public DataModel(long[,] timematr, long[,] timewind, int veh, int[] starts, int[] ends)
            {
                TimeMatrix = timematr;
                TimeWindows = timewind;
                VehicleNumber = veh;
                Starts = starts;
                Ends = ends;
            }
            public long[,] TimeMatrix;
            public long[,] TimeWindows;
            public int VehicleNumber;
            public int[] Starts;
            public int[] Ends;
        };

        static List<long[]> PrintSolution(in DataModel data, in RoutingModel routing, in RoutingIndexManager manager,
                                  in Assignment solution)
        {
            List<long[]> m = new List<long[]>();

            // Inspect solution.
            RoutingDimension timeDimension = routing.GetMutableDimension("Time");
            var index = routing.Start(0);
            while (routing.IsEnd(index) == false)
            {
                var timeVar = timeDimension.CumulVar(index);
                m.Add(new long[] { manager.IndexToNode(index), solution.Min(timeVar) });
                index = solution.Value(routing.NextVar(index));
            }
            var endTimeVar = timeDimension.CumulVar(index);
            m.Add(new long[] { manager.IndexToNode(index), solution.Min(endTimeVar) });
            return m;
        }

        public static List<long[]> FindSolution(DataModel data, long[] penalty, long maxtime)
        {
            // Create Routing Index Manager
            RoutingIndexManager manager =
                new RoutingIndexManager(data.TimeMatrix.GetLength(0), data.VehicleNumber, data.Starts, data.Ends);

            // Create Routing Model.
            RoutingModel routing = new RoutingModel(manager);

            // Create and register a transit callback.
            int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
            {
                // Convert from routing variable Index to time
                // matrix NodeIndex.
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);
                return data.TimeMatrix[fromNode, toNode];
            });

            // Define cost of each arc.
            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

            // Add Time constraint.
            routing.AddDimension(transitCallbackIndex, // transit callback
                                 0,                   // allow waiting time (30)
                                 1440,                   // vehicle maximum capacities (30) 2000
                                 false,                // start cumul to zero
                                 "Time");
            RoutingDimension timeDimension = routing.GetMutableDimension("Time");
            timeDimension.SetSpanUpperBoundForVehicle(maxtime, 0);
            // Add time window constraints for each location except depot.
            int n = data.TimeWindows.GetLength(0) - 1;
            for (int i = 1; i < n; ++i)
            {
                long index = manager.NodeToIndex(i);
                timeDimension.CumulVar(index).SetRange(data.TimeWindows[i, 0], data.TimeWindows[i, 1]);
            }
            // Add time window constraints for each vehicle start node.
            for (int i = 0; i < data.VehicleNumber; ++i)
            {
                long index = routing.Start(i);
                timeDimension.CumulVar(index).SetRange(data.TimeWindows[0, 0], data.TimeWindows[0, 1]);
                index = routing.End(i);
                timeDimension.CumulVar(index).SetRange(data.TimeWindows[n, 0], data.TimeWindows[n, 1]);
            }

            // Instantiate route start and end times to produce feasible times.
            for (int i = 0; i < data.VehicleNumber; ++i)
            {
                routing.AddVariableMinimizedByFinalizer(timeDimension.CumulVar(routing.Start(i)));
                routing.AddVariableMinimizedByFinalizer(timeDimension.CumulVar(routing.End(i)));
            }

            //penalty
            for (int i = 1; i < data.TimeMatrix.GetLength(0) - 1; ++i)
            {
                routing.AddDisjunction(new long[] { manager.NodeToIndex(i) }, penalty[i]);
            }


            // Setting first solution heuristic.
            RoutingSearchParameters searchParameters =
                operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            //searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.SimulatedAnnealing;
            //searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 30 };
            //searchParameters.LogSearch = true;
            //!!! LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;

            // Solve the problem.
            Assignment solution = routing.SolveWithParameters(searchParameters);

            // Print solution on console.
            return PrintSolution(data, routing, manager, solution);
        }
    }
}
