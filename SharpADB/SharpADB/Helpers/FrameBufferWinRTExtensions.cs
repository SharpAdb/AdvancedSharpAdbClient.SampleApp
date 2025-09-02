﻿// <copyright file="FrameBufferWinRTExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Models;
using SharpADB.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace SharpADB.Helpers
{
    /// <summary>
    /// Provides extension methods of <see cref="WriteableBitmap"/> for the <see cref="Framebuffer"/> and <see cref="FramebufferHeader"/> classes.
    /// </summary>
    public static class FrameBufferWinRTExtensions
    {
        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="framebuffer">The framebuffer data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static Task<WriteableBitmap> ToBitmapAsync(this Framebuffer framebuffer, CancellationToken cancellationToken = default)
        {
            framebuffer.EnsureNotDisposed();
            return framebuffer.Data == null ? throw new InvalidOperationException($"Call {nameof(framebuffer.RefreshAsync)} first") : framebuffer.Header.ToBitmapAsync(framebuffer.Data, cancellationToken);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="framebuffer">The framebuffer data.</param>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static Task<WriteableBitmap> ToBitmapAsync(this Framebuffer framebuffer, CoreDispatcher dispatcher, CancellationToken cancellationToken = default)
        {
            framebuffer.EnsureNotDisposed();
            return framebuffer.Data == null ? throw new InvalidOperationException($"Call {nameof(framebuffer.RefreshAsync)} first") : framebuffer.Header.ToBitmapAsync(framebuffer.Data, dispatcher, cancellationToken);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="framebuffer">The framebuffer data.</param>
        /// <param name="dispatcher">The target <see cref="DispatcherQueue"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        [SupportedOSPlatform("Windows10.0.16299.0")]
        [ContractVersion(typeof(UniversalApiContract), 327680u)]
        public static Task<WriteableBitmap> ToBitmapAsync(this Framebuffer framebuffer, DispatcherQueue dispatcher, CancellationToken cancellationToken = default)
        {
            framebuffer.EnsureNotDisposed();
            return framebuffer.Data == null ? throw new InvalidOperationException($"Call {nameof(framebuffer.RefreshAsync)} first") : framebuffer.Header.ToBitmapAsync(framebuffer.Data, dispatcher, cancellationToken);
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="header">The header containing the image metadata.</param>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static Task<WriteableBitmap> ToBitmapAsync(this in FramebufferHeader header, byte[] buffer, CoreDispatcher dispatcher, CancellationToken cancellationToken = default)
        {
            if (dispatcher.HasThreadAccess)
            {
                return header.ToBitmapAsync(buffer, cancellationToken);
            }
            else
            {
                FramebufferHeader self = header;
                TaskCompletionSource<WriteableBitmap> taskCompletionSource = new();
                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        WriteableBitmap result = await self.ToBitmapAsync(buffer, cancellationToken).ConfigureAwait(false);
                        _ = taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception e)
                    {
                        _ = taskCompletionSource.TrySetException(e);
                    }
                });
                return taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="header">The header containing the image metadata.</param>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="dispatcher">The target <see cref="DispatcherQueue"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [SupportedOSPlatform("Windows10.0.16299.0")]
        [ContractVersion(typeof(UniversalApiContract), 327680u)]
        public static Task<WriteableBitmap> ToBitmapAsync(this in FramebufferHeader header, byte[] buffer, DispatcherQueue dispatcher, CancellationToken cancellationToken = default)
        {
            if (ThreadSwitcher.IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                return header.ToBitmapAsync(buffer, cancellationToken);
            }
            else
            {
                FramebufferHeader self = header;
                TaskCompletionSource<WriteableBitmap> taskCompletionSource = new();
                if (!dispatcher.TryEnqueue(async () =>
                {
                    try
                    {
                        WriteableBitmap result = await self.ToBitmapAsync(buffer, cancellationToken).ConfigureAwait(false);
                        _ = taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception e)
                    {
                        _ = taskCompletionSource.TrySetException(e);
                    }
                }))
                {
                    _ = taskCompletionSource.TrySetException(new InvalidOperationException("Failed to enqueue the operation"));
                }
                return taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="header">The header containing the image metadata.</param>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static async Task<WriteableBitmap> ToBitmapAsync(this FramebufferHeader header, byte[] buffer, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            {
                return null;
            }

            // The pixel format of the framebuffer may not be one that WinRT recognizes, so we need to fix that
            BitmapPixelFormat bitmapPixelFormat = header.StandardizePixelFormat(buffer, out BitmapAlphaMode alphaMode);

            using InMemoryRandomAccessStream random = new();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random).AsTask(cancellationToken);
            encoder.SetPixelData(bitmapPixelFormat, alphaMode, header.Width, header.Height, 96, 96, buffer);
            await encoder.FlushAsync().AsTask(cancellationToken);
            WriteableBitmap WriteableImage = new((int)header.Width, (int)header.Height);
            await WriteableImage.SetSourceAsync(random).AsTask(cancellationToken).ConfigureAwait(false);

            return WriteableImage;
        }

        /// <summary>
        /// Returns the <see cref="BitmapPixelFormat"/> that describes pixel format of an image that is stored according to the information
        /// present in this <see cref="FramebufferHeader"/>. Because the <see cref="BitmapPixelFormat"/> enumeration does not allow for all
        /// formats supported by Android, this method also takes a <paramref name="buffer"/> and reorganizes the bytes in the buffer to
        /// match the return value of this function.
        /// </summary>
        /// <param name="header">The header containing the image metadata.</param>
        /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader"/>.</param>
        /// <param name="alphaMode">A <see cref="BitmapAlphaMode"/> which describes how the alpha channel is stored.</param>
        /// <returns>A <see cref="BitmapPixelFormat"/> that describes how the image data is represented in this <paramref name="buffer"/>.</returns>
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        private static BitmapPixelFormat StandardizePixelFormat(this in FramebufferHeader header, byte[] buffer, out BitmapAlphaMode alphaMode)
        {
            // Initial parameter validation.
            ArgumentNullException.ThrowIfNull(buffer);

            if (buffer.Length < header.Width * header.Height * (header.Bpp / 8))
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"The buffer length {buffer.Length} is less than expected buffer " +
                    $"length ({header.Width * header.Height * (header.Bpp / 8)}) for a picture of width {header.Width}, height {header.Height} and pixel depth {header.Bpp}");
            }

            if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            {
                throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");
            }

            // By far, the most common format is a 32-bit pixel format, which is either
            // RGB or RGBA, where each color has 1 byte.
            if (header.Bpp == 8 * 4)
            {
                // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
                if (header.Red.Length != 8 || header.Blue.Length != 8 || header.Green.Length != 8)
                {
                    throw new ArgumentOutOfRangeException($"The pixel format with with RGB lengths of {header.Red.Length}:{header.Blue.Length}:{header.Green.Length} is not supported");
                }

                // Alpha can be present or absent, but must be 8 bytes long
                alphaMode = header.Alpha.Length switch
                {
                    0 => BitmapAlphaMode.Ignore,
                    8 => BitmapAlphaMode.Straight,
                    _ => throw new ArgumentOutOfRangeException($"The alpha length {header.Alpha.Length} is not supported"),
                };

                return BitmapPixelFormat.Rgba8;
            }

            // If not caught by any of the statements before, the format is not supported.
            throw new NotSupportedException($"Pixel depths of {header.Bpp} are not supported");
        }

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(EnsureNotDisposed))]
        private static extern void EnsureNotDisposed(this Framebuffer framebuffer);
    }
}