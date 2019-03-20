using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FirstComponentTest
{
    public class MeshRecursion : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MeshRecursion()
          : base("Mesh Faces Aggregation", "FaceAgg",
              "Aggregate mesh faces",
              "FunAlgorithms", "Meshes")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "m", "Initial mesh for lunching algorithm", GH_ParamAccess.item);
            pManager.AddNumberParameter("displacement", "disp", "Displacement distance from where the points will move", GH_ParamAccess.item);
            pManager.AddIntegerParameter("iter", "iter", "iter number", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "aggregated mesh", "Output mesh after aggregating the faces", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double disp = 0;
            int iter = 1;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref disp)) return;
            if (!DA.GetData(2, ref iter)) return;

            if (!mesh.IsValid) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please input a valid mesh"); return; };
            if (iter == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Iter must be greater than zero");return; };

            Mesh subdividedMesh = new Mesh();

            for (int i = 0; i < iter; i++)
            {
            subdividedMesh = new Mesh();
            subdividedMesh.Vertices.AddVertices(mesh.Vertices);

                for (int count = 0; count < mesh.Faces.Count; count++)
                {
                    Point3d faceCenter = mesh.Faces.GetFaceCenter(count);
                    Vector3d normals = mesh.NormalAt(mesh.ClosestMeshPoint(faceCenter, 0.0));
                    faceCenter += (normals * disp);
                    subdividedMesh.Vertices.Add(faceCenter);
                    
                    subdividedMesh.Faces.AddFace(mesh.Faces[count].A, mesh.Faces[count].B, subdividedMesh.Vertices.Count - 1);
                    subdividedMesh.Faces.AddFace(mesh.Faces[count].B, mesh.Faces[count].C, subdividedMesh.Vertices.Count - 1);
                    if (mesh.Faces[count].IsQuad)
                    {
                        subdividedMesh.Faces.AddFace(mesh.Faces[count].C, mesh.Faces[count].D, subdividedMesh.Vertices.Count - 1);
                        subdividedMesh.Faces.AddFace(mesh.Faces[count].D, mesh.Faces[count].A, subdividedMesh.Vertices.Count - 1);
                    }
                    else
                    {
                        subdividedMesh.Faces.AddFace(mesh.Faces[count].C, mesh.Faces[count].A, subdividedMesh.Vertices.Count - 1);
                    }
                }
                subdividedMesh.Compact();
                subdividedMesh.RebuildNormals();
                mesh = subdividedMesh;
            }

            DA.SetData(0, subdividedMesh);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b0e5cdb0-93b4-4170-b092-78ce8b4d9575"); }
        }
    }
}