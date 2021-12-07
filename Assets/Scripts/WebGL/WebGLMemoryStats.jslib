var MemoryStatsPlugin = {


  GetMaxMemorySize: function() {
    return performance.memory.jsHeapSizeLimit; // WebGLMemorySize in bytes
  },
  GetCurrentMemorySize: function() {
    return performance.memory.totalJSHeapSize; // WebGLMemorySize in bytes
  },
  GetUsedMemorySize: function() {
    return HEAP8.length; // WebGLMemorySize in bytes
  }
};

mergeInto(LibraryManager.library, MemoryStatsPlugin);
