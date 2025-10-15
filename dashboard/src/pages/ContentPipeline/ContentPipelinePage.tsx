import PageHeader from '../../components/common/PageHeader';
import './ContentPipelinePage.css';

const ContentPipelinePage = () => {
  return (
    <div className="content-pipeline">
      <PageHeader
        eyebrow="Workflow"
        title="Content pipeline"
        description="Track drafting, approval, and scheduling status across channels. API integration coming next."
      />

      <section className="content-pipeline__placeholder">
        <p>Connect this view to the content pipeline API once endpoints are ready.</p>
      </section>
    </div>
  );
};

export default ContentPipelinePage;
