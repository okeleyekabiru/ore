import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export interface BrandSurvey {
  id: string;
  name: string;
  brandVoice: {
    tone: string;
    voice: string;
    audience: string;
    keywords: string[];
    goals: string;
    competitors: string[];
  };
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

interface SurveyState {
  surveys: BrandSurvey[];
  currentSurvey: BrandSurvey | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: SurveyState = {
  surveys: [],
  currentSurvey: null,
  isLoading: false,
  error: null,
};

// Async thunks (will be implemented with API calls)
export const fetchSurveys = createAsyncThunk(
  'surveys/fetchSurveys',
  async () => {
    // TODO: Replace with actual API call
    return [] as BrandSurvey[];
  }
);

export const createSurvey = createAsyncThunk(
  'surveys/createSurvey',
  async (surveyData: Omit<BrandSurvey, 'id' | 'createdAt' | 'updatedAt'>) => {
    // TODO: Replace with actual API call
    const newSurvey: BrandSurvey = {
      id: Math.random().toString(),
      ...surveyData,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    return newSurvey;
  }
);

const surveySlice = createSlice({
  name: 'surveys',
  initialState,
  reducers: {
    clearCurrentSurvey: (state) => {
      state.currentSurvey = null;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch surveys
      .addCase(fetchSurveys.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchSurveys.fulfilled, (state, action) => {
        state.isLoading = false;
        state.surveys = action.payload;
      })
      .addCase(fetchSurveys.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch surveys';
      })
      // Create survey
      .addCase(createSurvey.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createSurvey.fulfilled, (state, action) => {
        state.isLoading = false;
        state.surveys.push(action.payload);
        state.currentSurvey = action.payload;
      })
      .addCase(createSurvey.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to create survey';
      });
  },
});

export const { clearCurrentSurvey, clearError } = surveySlice.actions;
export default surveySlice.reducer;