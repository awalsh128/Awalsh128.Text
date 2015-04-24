/*
 * Copyright (C) 2015 Andrew Walsh
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License (LGPL)
 * version 2 as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/
namespace Awalsh128.Text
{
    using System;

    internal static class ObjectExtensions
    {
        /// <summary>
        /// Throw an argument exception if item doesn't satisfy requirement.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="item">The object to check.</param>
        /// <param name="argumentName">The argument name pointing to the object.</param>
        /// <param name="requirement">The requirement on the object.</param>
        /// <param name="message">The exception message to report if the object doesn't satisfy the requirement.</param>
        internal static void ThrowIfInvalid<T>(
            this T item,
            string argumentName,
            Func<T, bool> requirement,
            string message = null)
        {
            if (!requirement(item))
            {
                throw new ArgumentException(
                    string.Format("{0} {1}", argumentName, (string.IsNullOrEmpty(message) ? "is invalid" : message)));
            }
        }
    }
}