import PageHeader from '../../components/common/PageHeader';
import './OverviewPage.css';

const stats = [
  {
    label: 'Posts scheduled',
    value: '24',
    delta: '+6 since last week',
  },
  {
    label: 'Awaiting approval',
    value: '5',
    delta: '2 require manager review',
  },
  {
    label: 'LLM usage',
    value: '78 prompts',
    delta: 'Autopilot handled 63%',
  },
];

const milestones = [
  {
    title: 'Onboard Acme organics',
    date: 'Due Oct 22',
    summary: 'Brand survey 75% complete · Needs tone samples',
  },
  {
    title: 'Campaign: Winter drops',
    date: 'Kickoff Oct 25',
    summary: 'Prompt templates drafted · Awaiting legal copy update',
  },
];

const queueItems = [
  {
    title: 'Instagram carousel — Product launch',
    stage: 'Ready for approval',
    owner: 'Bayo | IG',
  },
  {
    title: 'LinkedIn article — Founder spotlight',
    stage: 'Drafting',
    owner: 'Amelia | LI',
  },
  {
    title: 'TikTok script — BTS series',
    stage: 'Awaiting assets',
    owner: 'Kai | TikTok',
  },
];

const OverviewPage = () => {
  return (
    <div className="overview">
      <PageHeader
        eyebrow="Overview"
        title="Team pulse & pipeline health"
        description="High-level telemetry across onboarding, generation, and publishing so you can unblock work before deadlines slip."
        actions={
          <button type="button" className="button button--primary">
            New content request
          </button>
        }
      />

      <section className="grid grid--three overview__stat-grid" aria-label="Key metrics">
        {stats.map((stat) => (
          <article key={stat.label} className="card overview__stat-card">
            <span className="overview__stat-label">{stat.label}</span>
            <span className="overview__stat-value">{stat.value}</span>
            <span className="overview__stat-delta">{stat.delta}</span>
          </article>
        ))}
      </section>

      <section className="overview__panels">
        <article className="card overview__panel" aria-label="Upcoming milestones">
          <h3 className="section-title">Upcoming milestones</h3>
          <ul>
            {milestones.map((milestone) => (
              <li key={milestone.title}>
                <div>
                  <strong>{milestone.title}</strong>
                  <span>{milestone.summary}</span>
                </div>
                <time>{milestone.date}</time>
              </li>
            ))}
          </ul>
        </article>

        <article className="card overview__panel" aria-label="Workflow queue">
          <h3 className="section-title">Pipeline snapshot</h3>
          <ul>
            {queueItems.map((item) => (
              <li key={item.title}>
                <div>
                  <strong>{item.title}</strong>
                  <span>{item.stage}</span>
                </div>
                <span className="overview__queue-owner">{item.owner}</span>
              </li>
            ))}
          </ul>
        </article>
      </section>
    </div>
  );
};

export default OverviewPage;
