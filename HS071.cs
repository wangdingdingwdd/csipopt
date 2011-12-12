// Copyright (C) 2010 Anders Gustafsson and others. All Rights Reserved.
// This code is published under the Eclipse Public License.
//
// Author:  Anders Gustafsson, Cureos AB 2011-09-02

using System;
using System.Runtime.InteropServices;

namespace Cureos.Numerics
{
    public class HS071
    {
        public int _n;
        public int _m;
        public int _nele_jac;
        public int _nele_hess;
        public double[] _x_L;
        public double[] _x_U;
        public double[] _g_L;
        public double[] _g_U;

        public HS071()
        {
            /* set the number of variables and allocate space for the bounds */
            /* set the values for the variable bounds */
            _n = 4;
            _x_L = new[] { 1.0, 1.0, 1.0, 1.0 };
            _x_U = new[] { 5.0, 5.0, 5.0, 5.0 };

            /* set the number of constraints and allocate space for the bounds */
            _m = 2;

            /* set the values of the constraint bounds */
            _g_L = new[] { 25.0, 40.0 };
            _g_U = new[] { CsIpopt.PositiveInfinity, 40.0 };

            /* Number of nonzeros in the Jacobian of the constraints */
            _nele_jac = 8;

            /* Number of nonzeros in the Hessian of the Lagrangian (lower or
               upper triangual part only) */
            _nele_hess = 10;
        }

        [AllowReversePInvokeCalls]
        public int eval_f(int n, double[] x, int new_x, out double obj_value, IntPtr user_data)
        {
            obj_value = x[0] * x[3] * (x[0] + x[1] + x[2]) + x[2];

            return CsIpopt.TRUE;
        }

        [AllowReversePInvokeCalls]
        public int eval_grad_f(int n, double[] x, int new_x, double[] grad_f, IntPtr user_data)
        {
            grad_f[0] = x[0] * x[3] + x[3] * (x[0] + x[1] + x[2]);
            grad_f[1] = x[0] * x[3];
            grad_f[2] = x[0] * x[3] + 1;
            grad_f[3] = x[0] * (x[0] + x[1] + x[2]);

            return CsIpopt.TRUE;
        }

        [AllowReversePInvokeCalls]
        public int eval_g(int n, double[] x, int new_x, int m, double[] g, IntPtr user_data)
        {
            g[0] = x[0] * x[1] * x[2] * x[3];
            g[1] = x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3];

            return CsIpopt.TRUE;
        }

        [AllowReversePInvokeCalls]
        public int eval_jac_g(int n, double[] x, int new_x, int m, int nele_jac, int[] iRow, int[] jCol, double[] values, IntPtr user_data)
        {
            if (values == null)
            {
                /* set the structure of the jacobian */
                /* this particular jacobian is dense */

                iRow[0] = 0;
                jCol[0] = 0;
                iRow[1] = 0;
                jCol[1] = 1;
                iRow[2] = 0;
                jCol[2] = 2;
                iRow[3] = 0;
                jCol[3] = 3;
                iRow[4] = 1;
                jCol[4] = 0;
                iRow[5] = 1;
                jCol[5] = 1;
                iRow[6] = 1;
                jCol[6] = 2;
                iRow[7] = 1;
                jCol[7] = 3;
            }
            else
            {
                /* return the values of the jacobian of the constraints */

                values[0] = x[1] * x[2] * x[3]; /* 0,0 */
                values[1] = x[0] * x[2] * x[3]; /* 0,1 */
                values[2] = x[0] * x[1] * x[3]; /* 0,2 */
                values[3] = x[0] * x[1] * x[2]; /* 0,3 */

                values[4] = 2 * x[0];         /* 1,0 */
                values[5] = 2 * x[1];         /* 1,1 */
                values[6] = 2 * x[2];         /* 1,2 */
                values[7] = 2 * x[3];         /* 1,3 */
            }

            return CsIpopt.TRUE;
        }

        [AllowReversePInvokeCalls]
        public int eval_h(int n, double[] x, int new_x, double obj_factor,
                    int m, double[] lambda, int new_lambda,
                    int nele_hess, int[] iRow, int[] jCol,
                    double[] values, IntPtr user_data)
        {
            if (values == null)
            {
                /* set the Hessian structure. This is a symmetric matrix, fill the lower left
                 * triangle only. */

                /* the hessian for this problem is actually dense */
                int idx = 0; /* nonzero element counter */
                for (int row = 0; row < 4; row++)
                {
                    for (int col = 0; col <= row; col++)
                    {
                        iRow[idx] = row;
                        jCol[idx] = col;
                        idx++;
                    }
                }

            }
            else
            {
                /* return the values. This is a symmetric matrix, fill the lower left
                 * triangle only */

                /* fill the objective portion */
                values[0] = obj_factor * (2 * x[3]);               /* 0,0 */

                values[1] = obj_factor * (x[3]);                 /* 1,0 */
                values[2] = 0;                                   /* 1,1 */

                values[3] = obj_factor * (x[3]);                 /* 2,0 */
                values[4] = 0;                                   /* 2,1 */
                values[5] = 0;                                   /* 2,2 */

                values[6] = obj_factor * (2 * x[0] + x[1] + x[2]); /* 3,0 */
                values[7] = obj_factor * (x[0]);                 /* 3,1 */
                values[8] = obj_factor * (x[0]);                 /* 3,2 */
                values[9] = 0;                                   /* 3,3 */


                /* add the portion for the first constraint */
                values[1] += lambda[0] * (x[2] * x[3]);          /* 1,0 */

                values[3] += lambda[0] * (x[1] * x[3]);          /* 2,0 */
                values[4] += lambda[0] * (x[0] * x[3]);          /* 2,1 */

                values[6] += lambda[0] * (x[1] * x[2]);          /* 3,0 */
                values[7] += lambda[0] * (x[0] * x[2]);          /* 3,1 */
                values[8] += lambda[0] * (x[0] * x[1]);          /* 3,2 */

                /* add the portion for the second constraint */
                values[0] += lambda[1] * 2;                      /* 0,0 */

                values[2] += lambda[1] * 2;                      /* 1,1 */

                values[5] += lambda[1] * 2;                      /* 2,2 */

                values[9] += lambda[1] * 2;                      /* 3,3 */
            }

            return CsIpopt.TRUE;
        }

#if INTERMEDIATE
        [AllowReversePInvokeCalls]
        public int intermediate(int alg_mod, int iter_count, double obj_value, double inf_pr, double inf_du,
            double mu, double d_norm, double regularization_size, double alpha_du, double alpha_pr, int ls_trials, IntPtr user_data)
        {
            Console.WriteLine("Intermediate callback method at iteration {0} in {1} with d_norm {2}",
                iter_count, (IpoptAlgorithmMode)alg_mod, d_norm);
            return iter_count < 5 ? Ipopt.TRUE : Ipopt.FALSE;
        }
#endif

    }
}