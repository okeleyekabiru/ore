import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';

export interface ContentItem {
  id: string;
  title: string;
  body: string;
  platform: 'Meta' | 'LinkedIn' | 'X';
  status: 'Draft' | 'PendingApproval' | 'Approved' | 'Rejected' | 'Scheduled' | 'Published';
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  scheduledFor?: string;
  approvedBy?: string;
  approvedAt?: string;
  rejectionReason?: string;
}

interface ContentState {
  items: ContentItem[];
  currentItem: ContentItem | null;
  isLoading: boolean;
  isGenerating: boolean;
  error: string | null;
  filters: {
    status: string;
    platform: string;
  };
}

const initialState: ContentState = {
  items: [],
  currentItem: null,
  isLoading: false,
  isGenerating: false,
  error: null,
  filters: {
    status: 'all',
    platform: 'all',
  },
};

// Async thunks
export const generateContent = createAsyncThunk(
  'content/generateContent',
  async (prompt: { topic: string; platform: string; tone: string }) => {
    // TODO: Replace with actual API call
    const generatedContent: Omit<ContentItem, 'id' | 'createdBy' | 'createdAt' | 'updatedAt'> = {
      title: `Generated content about ${prompt.topic}`,
      body: `This is AI-generated content about ${prompt.topic} for ${prompt.platform} with a ${prompt.tone} tone.`,
      platform: prompt.platform as 'Meta' | 'LinkedIn' | 'X',
      status: 'Draft',
    };
    
    return {
      id: Math.random().toString(),
      ...generatedContent,
      createdBy: 'current-user',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    } as ContentItem;
  }
);

export const fetchContent = createAsyncThunk(
  'content/fetchContent',
  async () => {
    // TODO: Replace with actual API call
    return [] as ContentItem[];
  }
);

export const submitForApproval = createAsyncThunk(
  'content/submitForApproval',
  async (contentId: string) => {
    // TODO: Replace with actual API call
    return contentId;
  }
);

const contentSlice = createSlice({
  name: 'content',
  initialState,
  reducers: {
    setCurrentItem: (state, action: PayloadAction<ContentItem | null>) => {
      state.currentItem = action.payload;
    },
    updateCurrentItem: (state, action: PayloadAction<Partial<ContentItem>>) => {
      if (state.currentItem) {
        state.currentItem = { ...state.currentItem, ...action.payload };
      }
    },
    setFilters: (state, action: PayloadAction<{ status?: string; platform?: string }>) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Generate content
      .addCase(generateContent.pending, (state) => {
        state.isGenerating = true;
        state.error = null;
      })
      .addCase(generateContent.fulfilled, (state, action) => {
        state.isGenerating = false;
        state.currentItem = action.payload;
        state.items.unshift(action.payload);
      })
      .addCase(generateContent.rejected, (state, action) => {
        state.isGenerating = false;
        state.error = action.error.message || 'Failed to generate content';
      })
      // Fetch content
      .addCase(fetchContent.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchContent.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload;
      })
      .addCase(fetchContent.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch content';
      })
      // Submit for approval
      .addCase(submitForApproval.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(submitForApproval.fulfilled, (state, action) => {
        state.isLoading = false;
        const item = state.items.find(item => item.id === action.payload);
        if (item) {
          item.status = 'PendingApproval';
          item.updatedAt = new Date().toISOString();
        }
      })
      .addCase(submitForApproval.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to submit for approval';
      });
  },
});

export const { setCurrentItem, updateCurrentItem, setFilters, clearError } = contentSlice.actions;
export default contentSlice.reducer;