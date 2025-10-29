import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { ContentItem } from './contentSlice';

export interface ApprovalItem extends ContentItem {
  submittedAt: string;
  submittedBy: string;
}

interface ApprovalState {
  pendingItems: ApprovalItem[];
  isLoading: boolean;
  error: string | null;
}

const initialState: ApprovalState = {
  pendingItems: [],
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchPendingApprovals = createAsyncThunk(
  'approvals/fetchPending',
  async () => {
    // TODO: Replace with actual API call
    return [] as ApprovalItem[];
  }
);

export const approveContent = createAsyncThunk(
  'approvals/approve',
  async (contentId: string) => {
    // TODO: Replace with actual API call
    return contentId;
  }
);

export const rejectContent = createAsyncThunk(
  'approvals/reject',
  async ({ contentId, reason }: { contentId: string; reason: string }) => {
    // TODO: Replace with actual API call
    return { contentId, reason };
  }
);

const approvalSlice = createSlice({
  name: 'approvals',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch pending approvals
      .addCase(fetchPendingApprovals.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchPendingApprovals.fulfilled, (state, action) => {
        state.isLoading = false;
        state.pendingItems = action.payload;
      })
      .addCase(fetchPendingApprovals.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch pending approvals';
      })
      // Approve content
      .addCase(approveContent.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(approveContent.fulfilled, (state, action) => {
        state.isLoading = false;
        state.pendingItems = state.pendingItems.filter(item => item.id !== action.payload);
      })
      .addCase(approveContent.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to approve content';
      })
      // Reject content
      .addCase(rejectContent.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(rejectContent.fulfilled, (state, action) => {
        state.isLoading = false;
        state.pendingItems = state.pendingItems.filter(item => item.id !== action.payload.contentId);
      })
      .addCase(rejectContent.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to reject content';
      });
  },
});

export const { clearError } = approvalSlice.actions;
export default approvalSlice.reducer;