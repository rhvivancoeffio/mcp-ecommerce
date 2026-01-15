import React, { useState, useCallback } from 'react';

interface ImageCarouselProps {
  images: string[];
  alt: string;
  className?: string;
  showIndicators?: boolean;
  showNavigation?: boolean;
  autoPlay?: boolean;
  autoPlayInterval?: number;
}

export const ImageCarousel: React.FC<ImageCarouselProps> = ({
  images,
  alt,
  className = '',
  showIndicators = true,
  showNavigation = true,
  autoPlay = false,
  autoPlayInterval = 3000,
}) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isHovered, setIsHovered] = useState(false);

  const goToPrevious = useCallback(() => {
    setCurrentIndex((prevIndex) => 
      prevIndex === 0 ? images.length - 1 : prevIndex - 1
    );
  }, [images.length]);

  const goToNext = useCallback(() => {
    setCurrentIndex((prevIndex) => 
      prevIndex === images.length - 1 ? 0 : prevIndex + 1
    );
  }, [images.length]);

  const goToSlide = useCallback((index: number) => {
    setCurrentIndex(index);
  }, []);

  // Auto-play functionality
  React.useEffect(() => {
    if (autoPlay && !isHovered && images.length > 1) {
      const interval = setInterval(() => {
        goToNext();
      }, autoPlayInterval);
      return () => clearInterval(interval);
    }
  }, [autoPlay, isHovered, images.length, autoPlayInterval, goToNext]);

  if (!images || images.length === 0) {
    return (
      <div className={`flex h-full w-full items-center justify-center bg-black/[0.02] ${className}`}>
        <div className="text-center">
          <svg className="mx-auto h-12 w-12 text-black/20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span className="mt-2 block text-sm text-black/40">No Image</span>
        </div>
      </div>
    );
  }

  if (images.length === 1) {
    return (
      <div className={`relative flex items-center justify-center ${className}`}>
        <img
          src={images[0]}
          alt={alt}
          className="h-full w-full object-contain"
          onError={(e) => {
            const target = e.target as HTMLImageElement;
            target.style.display = 'none';
            const parent = target.parentElement;
            if (parent && !parent.querySelector('.image-fallback')) {
              const fallback = document.createElement('div');
              fallback.className = 'image-fallback flex h-full w-full items-center justify-center bg-black/[0.02]';
              fallback.innerHTML = '<span class="text-sm text-black/40">No Image</span>';
              parent.appendChild(fallback);
            }
          }}
        />
      </div>
    );
  }

  return (
    <div
      className={`relative overflow-hidden ${className}`}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Images Container */}
      <div className="relative flex h-full w-full items-center justify-center">
        {images.map((image, index) => (
          <div
            key={index}
            className={`absolute inset-0 flex items-center justify-center transition-opacity duration-300 ${
              index === currentIndex ? 'opacity-100' : 'opacity-0'
            }`}
          >
            <img
              src={image}
              alt={`${alt} - Image ${index + 1}`}
              className="h-full w-full object-contain"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.style.display = 'none';
                const parent = target.parentElement;
                if (parent && !parent.querySelector('.image-fallback')) {
                  const fallback = document.createElement('div');
                  fallback.className = 'image-fallback flex h-full w-full items-center justify-center bg-black/[0.02]';
                  fallback.innerHTML = '<span class="text-sm text-black/40">No Image</span>';
                  parent.appendChild(fallback);
                }
              }}
            />
          </div>
        ))}
      </div>

      {/* Navigation Arrows */}
      {showNavigation && images.length > 1 && (
        <>
          <button
            type="button"
            onClick={goToPrevious}
            className={`absolute left-3 top-1/2 -translate-y-1/2 z-10 rounded-full bg-white/90 shadow-lg p-2.5 text-gray-700 transition-all hover:bg-white hover:shadow-xl hover:scale-110 ${
              isHovered ? 'opacity-100' : 'opacity-0'
            }`}
            aria-label="Previous image"
          >
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            type="button"
            onClick={goToNext}
            className={`absolute right-3 top-1/2 -translate-y-1/2 z-10 rounded-full bg-white/90 shadow-lg p-2.5 text-gray-700 transition-all hover:bg-white hover:shadow-xl hover:scale-110 ${
              isHovered ? 'opacity-100' : 'opacity-0'
            }`}
            aria-label="Next image"
          >
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </>
      )}

      {/* Indicators */}
      {showIndicators && images.length > 1 && (
        <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-10 flex gap-2">
          {images.map((_, index) => (
            <button
              key={index}
              type="button"
              onClick={() => goToSlide(index)}
              className={`rounded-full transition-all ${
                index === currentIndex
                  ? 'h-2 w-8 bg-white shadow-md'
                  : 'h-2 w-2 bg-white/60 hover:bg-white/80'
              }`}
              aria-label={`Go to image ${index + 1}`}
            />
          ))}
        </div>
      )}
    </div>
  );
};
