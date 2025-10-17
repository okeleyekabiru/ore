import { useCallback, useEffect, useMemo, useState } from 'react';
import PageHeader from '../../components/common/PageHeader';
import { useAuth } from '../../contexts/AuthContext';
import { contentPipelineService } from '../../services/contentPipelineService';
import { ApiError } from '../../types/api';
import {
  ContentPipelineStatus,
  type ContentPipelineItem,
  type ContentPipelinePageRequest,
  type ContentPipelineSummary,
} from '../../types/contentPipeline';
import './ContentPipelinePage.css';

const BOARD_COLUMNS = [
  {
    key: ContentPipelineStatus.Ideation,
    title: 'Ideation',
    blurb: 'Briefs and prompts awaiting kickoff',
  },
  {
    key: ContentPipelineStatus.InApproval,
    title: 'In approval',
    blurb: 'Routing through manager or legal review',
  },
  {
    key: ContentPipelineStatus.Scheduled,
    title: 'Scheduled',
    blurb: 'Queued and ready for distribution',
  },
] as const;

type BoardColumnKey = (typeof BOARD_COLUMNS)[number]['key'];
type BoardState = Record<ContentPipelineStatus, ContentPipelineItem[]>;
type FilterKey = 'all' | 'mine' | 'scheduled';

const STATUS_LABELS: Record<ContentPipelineStatus, string> = {
  [ContentPipelineStatus.Ideation]: 'Ideation',
  [ContentPipelineStatus.Drafting]: 'Drafting',
  [ContentPipelineStatus.InApproval]: 'In approval',
  [ContentPipelineStatus.Scheduled]: 'Scheduled',
  [ContentPipelineStatus.Published]: 'Published',
  [ContentPipelineStatus.NeedsRevision]: 'Needs revision',
};

const BOARD_STATUS_OPTIONS: ContentPipelineStatus[] = BOARD_COLUMNS.map((column) => column.key);

const FALLBACK_ITEMS: ContentPipelineItem[] = [
  {
    id: 'demo-ideation-1',
    teamId: 'demo-team',
    title: 'March product spotlight',
    status: ContentPipelineStatus.Ideation,
    channel: { id: 'linkedin', name: 'LinkedIn' },
    owner: { id: 'user-jordan', name: 'Jordan Lee' },
    updatedOnUtc: '2025-02-28T14:15:00Z',
    dueOnUtc: '2025-03-04T16:00:00Z',
  },
  {
    id: 'demo-ideation-2',
    teamId: 'demo-team',
    title: 'Q2 thought leadership series',
    status: ContentPipelineStatus.Ideation,
    channel: { id: 'blog', name: 'Blog' },
    owner: { id: 'user-priya', name: 'Priya Shah' },
    updatedOnUtc: '2025-02-27T10:00:00Z',
    dueOnUtc: '2025-03-08T13:00:00Z',
  },
  {
    id: 'demo-approval-1',
    teamId: 'demo-team',
    title: 'Customer spotlight reel',
    status: ContentPipelineStatus.InApproval,
    channel: { id: 'instagram', name: 'Instagram' },
    owner: { id: 'user-alex', name: 'Alex Chen' },
    updatedOnUtc: '2025-02-26T18:20:00Z',
    dueOnUtc: '2025-03-05T09:30:00Z',
  },
  {
    id: 'demo-scheduled-1',
    teamId: 'demo-team',
    title: 'Newsletter #58',
    status: ContentPipelineStatus.Scheduled,
    channel: { id: 'email', name: 'Email' },
    owner: { id: 'user-taylor', name: 'Taylor Morgan' },
    updatedOnUtc: '2025-02-25T11:45:00Z',
    scheduledOnUtc: '2025-03-06T14:00:00Z',
  },
  {
    id: 'demo-scheduled-2',
    teamId: 'demo-team',
    title: 'Spring launch announcement',
    status: ContentPipelineStatus.Scheduled,
    channel: { id: 'twitter', name: 'X (Twitter)' },
    owner: { id: 'user-jordan', name: 'Jordan Lee' },
    updatedOnUtc: '2025-02-24T09:12:00Z',
    scheduledOnUtc: '2025-03-09T15:30:00Z',
  },
];

const buildEmptyBoard = (): BoardState => ({
  [ContentPipelineStatus.Ideation]: [],
  [ContentPipelineStatus.Drafting]: [],
  [ContentPipelineStatus.InApproval]: [],
  [ContentPipelineStatus.Scheduled]: [],
  [ContentPipelineStatus.Published]: [],
  [ContentPipelineStatus.NeedsRevision]: [],
});

const groupItemsByStatus = (items: ContentPipelineItem[]): BoardState => {
  const board = buildEmptyBoard();
  items.forEach((item) => {
    if (board[item.status]) {
      board[item.status] = [...board[item.status], item];
    } else {
      board[ContentPipelineStatus.Ideation] = [...board[ContentPipelineStatus.Ideation], item];
    }
  });
  return board;
};

const formatDate = (iso: string) =>
  new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(iso));

const getDateBadge = (item: ContentPipelineItem) => {
  if (item.scheduledOnUtc) {
    return { label: 'Publishes', value: item.scheduledOnUtc };
  }

  if (item.dueOnUtc) {
    return { label: 'Due', value: item.dueOnUtc };
  }

  return { label: 'Updated', value: item.updatedOnUtc };
};

const ContentPipelinePage = () => {
  const { user } = useAuth();
  const [selectedFilter, setSelectedFilter] = useState<FilterKey>('all');
  const [viewType, setViewType] = useState<'board' | 'timeline'>('board');
  const [boardItems, setBoardItems] = useState<BoardState>(() => groupItemsByStatus(FALLBACK_ITEMS));
  const [summary, setSummary] = useState<ContentPipelineSummary[] | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>('Content pipeline API not available yet. Showing sample data.');
  const [transitioningId, setTransitioningId] = useState<string | null>(null);

  const chipClass = (isActive: boolean) =>
    isActive ? 'content-pipeline__chip content-pipeline__chip--active' : 'content-pipeline__chip';

  const countsByStatus = useMemo(() => {
    const counts: Record<ContentPipelineStatus, number> = {
      [ContentPipelineStatus.Ideation]: boardItems[ContentPipelineStatus.Ideation]?.length ?? 0,
      [ContentPipelineStatus.Drafting]: boardItems[ContentPipelineStatus.Drafting]?.length ?? 0,
      [ContentPipelineStatus.InApproval]: boardItems[ContentPipelineStatus.InApproval]?.length ?? 0,
      [ContentPipelineStatus.Scheduled]: boardItems[ContentPipelineStatus.Scheduled]?.length ?? 0,
      [ContentPipelineStatus.Published]: boardItems[ContentPipelineStatus.Published]?.length ?? 0,
      [ContentPipelineStatus.NeedsRevision]: boardItems[ContentPipelineStatus.NeedsRevision]?.length ?? 0,
    };

    summary?.forEach((entry) => {
      counts[entry.status] = entry.count;
    });

    return counts;
  }, [boardItems, summary]);

  const fetchPipeline = useCallback(
    async (activeFilter: FilterKey, abortSignal: AbortSignal) => {
      if (!user) {
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const requestBase: ContentPipelinePageRequest = {
          page: 1,
          pageSize: 25,
          teamId: user.teamId ?? undefined,
        };

        const itemsRequest: ContentPipelinePageRequest = { ...requestBase };

        if (activeFilter === 'mine' && user.userId) {
          itemsRequest.ownerId = user.userId;
        }

        if (activeFilter === 'scheduled') {
          itemsRequest.status = ContentPipelineStatus.Scheduled;
        }

        const [summaryResponse, itemsResponse] = await Promise.all([
          contentPipelineService.getSummary(requestBase.teamId),
          contentPipelineService.getItems(itemsRequest),
        ]);

        if (abortSignal.aborted) {
          return;
        }

        setSummary(summaryResponse);
        setBoardItems(groupItemsByStatus(itemsResponse.items));
      } catch (err) {
        if (abortSignal.aborted) {
          return;
        }

        console.warn('Content pipeline API not available yet; falling back to sample data.', err);
        const fallbackMessage =
          err instanceof ApiError
            ? err.message || 'Content pipeline API not available yet. Showing sample data.'
            : 'Content pipeline API not available yet. Showing sample data.';
        setError(fallbackMessage);
        setSummary(null);
        setBoardItems(groupItemsByStatus(FALLBACK_ITEMS));
      } finally {
        if (!abortSignal.aborted) {
          setIsLoading(false);
        }
      }
    },
    [user],
  );

  useEffect(() => {
    const controller = new AbortController();
    if (user) {
      void fetchPipeline(selectedFilter, controller.signal);
    }

    return () => {
      controller.abort();
    };
  }, [fetchPipeline, selectedFilter, user]);

  const handleStatusChange = useCallback(
    async (item: ContentPipelineItem, nextStatus: ContentPipelineStatus) => {
      if (item.status === nextStatus) {
        return;
      }

      setTransitioningId(item.id);
      try {
        await contentPipelineService.updateStatus(item.id, { status: nextStatus });

        setBoardItems((prev) => {
          const sourceItems = prev[item.status] ?? [];
          const targetItems = prev[nextStatus] ?? [];
          const updatedItem: ContentPipelineItem = {
            ...item,
            status: nextStatus,
            updatedOnUtc: new Date().toISOString(),
          };

          return {
            ...prev,
            [item.status]: sourceItems.filter((entry) => entry.id !== item.id),
            [nextStatus]: [...targetItems, updatedItem],
          };
        });

        setSummary((prev) => {
          if (!prev) {
            return prev;
          }

          const nextSummary = [...prev];
          const sourceIndex = nextSummary.findIndex((entry) => entry.status === item.status);
          if (sourceIndex >= 0) {
            nextSummary[sourceIndex] = {
              ...nextSummary[sourceIndex],
              count: Math.max(nextSummary[sourceIndex].count - 1, 0),
            };
          }

          const destIndex = nextSummary.findIndex((entry) => entry.status === nextStatus);
          if (destIndex >= 0) {
            nextSummary[destIndex] = {
              ...nextSummary[destIndex],
              count: nextSummary[destIndex].count + 1,
            };
          } else {
            nextSummary.push({ status: nextStatus, count: 1 });
          }

          return nextSummary;
        });
      } catch (err) {
        console.error('Failed to update content pipeline status', err);
        setError('Unable to update status yet. Please try again once the pipeline API is live.');
      } finally {
        setTransitioningId(null);
      }
    },
    [],
  );

  const renderColumn = (columnKey: BoardColumnKey) => {
    const columnItems = boardItems[columnKey] ?? [];
    const meta = BOARD_COLUMNS.find((column) => column.key === columnKey);
    const count = countsByStatus[columnKey] ?? columnItems.length;

    return (
      <article key={columnKey} className="content-pipeline__column">
        <header className="content-pipeline__column-header">
          <div>
            <h3>{meta?.title}</h3>
            <p>{meta?.blurb}</p>
          </div>
          <span className="content-pipeline__badge">{count}</span>
        </header>
        <ul className="content-pipeline__cards">
          {columnItems.map((item) => {
            const dateBadge = getDateBadge(item);
            return (
              <li key={item.id} className="content-pipeline__card">
                <div className="content-pipeline__card-header">
                  <h4>{item.title}</h4>
                  <select
                    className="content-pipeline__status-select"
                    value={item.status}
                    onChange={(event) => handleStatusChange(item, event.target.value as ContentPipelineStatus)}
                    disabled={transitioningId === item.id}
                  >
                    {BOARD_STATUS_OPTIONS.map((statusOption) => (
                      <option key={statusOption} value={statusOption}>
                        {STATUS_LABELS[statusOption]}
                      </option>
                    ))}
                  </select>
                </div>
                <dl>
                  <div>
                    <dt>Channel</dt>
                    <dd>{item.channel.name}</dd>
                  </div>
                  <div>
                    <dt>Owner</dt>
                    <dd>{item.owner.name}</dd>
                  </div>
                  <div>
                    <dt>{dateBadge.label}</dt>
                    <dd>{formatDate(dateBadge.value)}</dd>
                  </div>
                </dl>
              </li>
            );
          })}
        </ul>
        <footer className="content-pipeline__column-footer">
          <button type="button" className="content-pipeline__link-button">
            + Add content
          </button>
        </footer>
      </article>
    );
  };

  return (
    <div className="content-pipeline">
      <PageHeader
        eyebrow="Workflow"
        title="Content pipeline"
        description="Track drafting, approval, and scheduling status across channels. Pipeline endpoints wire in automatically once available."
      />

      <section className="content-pipeline__controls">
        <div className="content-pipeline__filters">
          <strong>Filters</strong>
          <div className="content-pipeline__filter-group">
            <button
              type="button"
              className={chipClass(selectedFilter === 'all')}
              onClick={() => setSelectedFilter('all')}
            >
              All teams
            </button>
            <button
              type="button"
              className={chipClass(selectedFilter === 'mine')}
              onClick={() => setSelectedFilter('mine')}
              disabled={!user?.userId}
            >
              My queue
            </button>
            <button
              type="button"
              className={chipClass(selectedFilter === 'scheduled')}
              onClick={() => setSelectedFilter('scheduled')}
            >
              Scheduled
            </button>
          </div>
        </div>
        <div className="content-pipeline__view-toggle">
          <span>View</span>
          <button
            type="button"
            className={chipClass(viewType === 'board')}
            onClick={() => setViewType('board')}
          >
            Board
          </button>
          <button type="button" className="content-pipeline__chip" disabled>
            Timeline
          </button>
        </div>
      </section>

      {isLoading && <div className="content-pipeline__loading">Loading pipeline data…</div>}
      {error && <div className="content-pipeline__alert">{error}</div>}

      {viewType === 'board' ? (
        <section className="content-pipeline__board">{BOARD_COLUMNS.map((column) => renderColumn(column.key))}</section>
      ) : (
        <section className="content-pipeline__timeline-placeholder">
          <p>Timeline view unlocks once scheduling endpoints expose publication windows.</p>
        </section>
      )}

      <section className="content-pipeline__empty-state">
        <h3>API wiring checklist</h3>
        <ul>
          <li>Fetch pipeline summary (counts by status)</li>
          <li>Load paged content items scoped to team and owner</li>
          <li>Support drag-and-drop status changes</li>
          <li>Trigger publish handoff to scheduling service</li>
        </ul>
      </section>
    </div>
  );
};

export default ContentPipelinePage;
