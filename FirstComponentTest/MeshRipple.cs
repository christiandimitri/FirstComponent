using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FirstComponentTest
{
    public class MeshRipple : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CDimi_MeshRipple class.
        /// </summary>
        public MeshRipple()
          : base("Mesh Ripple", "MeshRip",
              "Displace the points of the mesh",
              "FunAlgorithms", "Meshes")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "m", "Initial mesh for lunching algorithm", GH_ParamAccess.item);
            pManager.AddPointParameter("Attractor Points", "Pt", "Displacement reference points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Disp. distance", "D", "Displacement distance from where the points will move", GH_ParamAccess.item,1.0);
            pManager.AddIntegerParameter("Iterations", "i", "Number of displacement iterations to be computed", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "m", "Output displaced mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Disp values", "D", "dis computed to test if component is working", GH_ParamAccess.list);
            pManager.AddPointParameter("P", "P", "P", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Point3d> attractors = new List<Point3d>();
            double distance = 0;
            int iter = 0;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, attractors)) return;
            if (!DA.GetData(2, ref distance)) return;
            if (!DA.GetData(3, ref iter)) return;

            // We should now validate the data and warn the user if invalid data is supplied.

            if (!mesh.IsValid) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please input a valid mesh");return; }

            if (iter == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Iteration count CANNOT be 0!"); return; }

            if (distance == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Distance count CANNOT be 0!"); return; }

            // Deform the mesh

            Mesh iterMesh = mesh;
            List<double> x = new List<double>();
            List<Point3d> y = new List<Point3d>();
            for (int i=0; i < iter; i++) iterMesh = DeformMeshByAttractors(iterMesh, attractors, distance, out x, out y);

            // Finally output your resulting mesh
            DA.SetData(0,iterMesh);
            DA.SetDataList(1, x);
            DA.SetDataList(2, y);
        }

        
        public Mesh DeformMeshByAttractors(Mesh mesh, List<Point3d> attractors, double distance) {
            List<double> x = new List<double>();
            List<Point3d> y = new List<Point3d>();
            return DeformMeshByAttractors(mesh, attractors, distance, out x, out y);
        }

        public Mesh DeformMeshByAttractors(Mesh mesh, List<Point3d> attractors, double distance, out List<double> displacementValues, out List<Point3d> resultingVertices)
        {
            // DO SOMETHING HERE
            List<double> dispV = new List<double>();
            List<Point3d> movedVertices = new List<Point3d>();

            mesh.RebuildNormals();

            foreach (Point3d vertex in mesh.Vertices)
            {
                double holder = 0.0;
                foreach (Point3d attractor in attractors)
                {
                    double displacement = computeDisplacement(vertex, attractor, distance);
                    holder = holder + displacement;

                }
                dispV.Add(holder / attractors.Count);
                Vector3d normal = mesh.NormalAt(mesh.ClosestMeshPoint(vertex, 0.0));
                movedVertices.Add(vertex + (normal * holder));
            }
             
            // Create new mesh  with moved vertices and original topology
            Mesh newMesh = new Mesh();
            newMesh.Vertices.AddVertices(movedVertices);
            newMesh.Faces.AddFaces(mesh.Faces);
            newMesh.Normals.ComputeNormals();
            newMesh.Compact();

            displacementValues = dispV;
            resultingVertices = movedVertices;
            return newMesh;
        }

        public double computeDisplacement(Point3d point, Point3d attractor, double waveHeight)
        {
            double distance = point.DistanceTo(attractor);
            return waveHeight * ((Math.Sin(distance) / distance));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return FirstComponentTest.Properties.Resources.MeshRippleLogo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("70ee2922-0793-4394-9ac3-dee5f5291b36"); }
        }
    }
}