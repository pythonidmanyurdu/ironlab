﻿// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
using SlimDX;
using SlimDX.Direct3D9;
using System.Windows.Media.Media3D;
#if ILNumerics
using ILNumerics;
using ILNumerics.Storage;
using ILNumerics.BuiltInFunctions;
#endif

namespace IronPlot.Plotting3D
{   
    /// <summary>
    /// Geometric primitive class for surfaces from ILArrays.
    /// </summary>
    public partial class SurfaceModel3D : Model3D
    {
        /// <summary>
        /// The general case: smooth or faceted and any colouring scheme
        /// </summary>
        protected unsafe void UpdateVertsAndIndsGeneral(bool updateVerticesOnly, bool oneSided)
        {
            Vector3[] tempVertexPositions;
            Vector3[] tempVertexNormals;
            int index = 0;
            Point3D worldPoint;
            int numVertices;
            numVertices = 6 * (lengthU - 1) * (lengthV - 1);
            if (vertices.Length != numVertices) vertices = new VertexPositionNormalColor[numVertices];
            // First, obtain all vertices in World space 
            tempVertexPositions = new Vector3[lengthU * lengthV];
            tempVertexNormals = new Vector3[lengthU * lengthV];
            MatrixTransform3D modelToWorld = ModelToWorld;
            for (int v = 0; v < lengthV; v++)
            {
                for (int u = 0; u < lengthU; u++)
                {
                    worldPoint = modelToWorld.Transform(modelVertices[index]);
                    tempVertexPositions[index] = new Vector3((float)worldPoint.X, (float)worldPoint.Y, (float)worldPoint.Z);
                    index++;
                }
            }
            // Next, go through all triangles, assigning vertices and indices
            // If shading is faceted then also assign normals. If smooth, then work out contribution to the 
            // shared normals and then assign           
            index = 0;
            int currentVertInd = 0;
            Vector3 point1, point2, point3, point4;
            Vector3 normal1, normal2, normal3, normal4;
            if (SurfaceShading == SurfaceShading.Smooth)
            {
                for (int v = 0; v < lengthV - 1; v++)
                {
                    for (int u = 0; u < lengthU - 1; u++)
                    {
                        point1 = tempVertexPositions[index];
                        point2 = tempVertexPositions[index + 1];
                        point3 = tempVertexPositions[index + lengthU + 1];
                        point4 = tempVertexPositions[index + lengthU];
                        normal1 = Vector3.Cross(point3 - point2, point1 - point2);
                        normal2 = Vector3.Cross(point1 - point4, point3 - point4);
                        normal1 += normal2;
                        //
                        tempVertexNormals[index] += normal1 + normal2;
                        tempVertexNormals[index + 1] += normal1;
                        tempVertexNormals[index + lengthU + 1] += normal1 + normal2;
                        tempVertexNormals[index + lengthU] += normal2;
                        // First triangle
                        vertices[currentVertInd + 0].Position = point1;
                        vertices[currentVertInd + 1].Position = point2;
                        vertices[currentVertInd + 2].Position = point3;
                        // Second triangle
                        vertices[currentVertInd + 3].Position = point3;
                        vertices[currentVertInd + 4].Position = point4;
                        vertices[currentVertInd + 5].Position = point1;
                        currentVertInd += 6;
                        index++;
                    }
                    index++;
                }
                index = 0;
                currentVertInd = 0;
                for (int v = 0; v < lengthV - 1; v++)
                {
                    for (int u = 0; u < lengthU - 1; u++)
                    {
                        normal1 = tempVertexNormals[index];
                        normal2 = tempVertexNormals[index + 1];
                        normal3 = tempVertexNormals[index + lengthU + 1];
                        normal4 = tempVertexNormals[index + lengthU];
                        vertices[currentVertInd + 0].Normal = normal1;
                        vertices[currentVertInd + 1].Normal = normal2;
                        vertices[currentVertInd + 2].Normal = normal3;
                        // Second triangle
                        vertices[currentVertInd + 3].Normal = normal3;
                        vertices[currentVertInd + 4].Normal = normal4;
                        vertices[currentVertInd + 5].Normal = normal1;
                        currentVertInd += 6;
                        index++;
                    }
                }
            }
            else
            {
                for (int v = 0; v < lengthV - 1; v++)
                {
                    for (int u = 0; u < lengthU - 1; u++)
                    {
                        point1 = tempVertexPositions[index];
                        point2 = tempVertexPositions[index + 1];
                        point3 = tempVertexPositions[index + lengthU + 1];
                        point4 = tempVertexPositions[index + lengthU];
                        normal1 = Vector3.Cross(point3 - point2, point1 - point2);
                        normal2 = Vector3.Cross(point1 - point4, point3 - point4);
                        normal1 += normal2;
                        // First triangle
                        vertices[currentVertInd].Position = point1; vertices[currentVertInd].Normal = normal1;
                        vertices[currentVertInd + 1].Position = point2; vertices[currentVertInd + 1].Normal = normal1;
                        vertices[currentVertInd + 2].Position = point3; vertices[currentVertInd + 2].Normal = normal1;
                        // Second triangle
                        vertices[currentVertInd + 3].Position = point3; vertices[currentVertInd + 3].Normal = normal1;
                        vertices[currentVertInd + 4].Position = point4; vertices[currentVertInd + 4].Normal = normal1;
                        vertices[currentVertInd + 5].Position = point1; vertices[currentVertInd + 5].Normal = normal1;
                        currentVertInd += 6;
                        index++;
                    }
                    index++;
                }
            }
            currentVertInd = 0;
            if (!updateVerticesOnly)
            {
                int currentInd = 0;
                for (int v = 0; v < lengthV - 1; v++)
                {
                    for (int u = 0; u < lengthU - 1; u++)
                    {
                        // First triangle
                        indices[currentInd] = currentVertInd + 2;
                        indices[currentInd + 1] = currentVertInd + 1;
                        indices[currentInd + 2] = currentVertInd + 0;
                        // Second triangle
                        indices[currentInd + 3] = currentVertInd + 5;
                        indices[currentInd + 4] = currentVertInd + 4;
                        indices[currentInd + 5] = currentVertInd + 3;
                        currentVertInd += 6;
                        currentInd += 6;
                    }
                }
                if (!oneSided)
                {
                    currentVertInd = 0;
                    for (int v = 0; v < lengthV - 1; v++)
                    {
                        for (int u = 0; u < lengthU - 1; u++)
                        {
                            // First triangle
                            indices[currentInd] = currentVertInd + 0;
                            indices[currentInd + 1] = currentVertInd + 1;
                            indices[currentInd + 2] = currentVertInd + 2;
                            // Second triangle
                            indices[currentInd + 3] = currentVertInd + 3;
                            indices[currentInd + 4] = currentVertInd + 4;
                            indices[currentInd + 5] = currentVertInd + 5;
                            currentVertInd += 6;
                            currentInd += 6;
                        }
                    }
                }
            }
        }
    }
}
