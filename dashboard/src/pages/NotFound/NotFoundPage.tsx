import { Link } from 'react-router-dom';
import './NotFoundPage.css';

const NotFoundPage = () => {
  return (
    <section className="not-found">
      <h2>We could not find that view</h2>
      <p>The route you tried to access does not exist. Use the navigation to continue.</p>
      <Link to="/" className="button button--primary">
        Back to overview
      </Link>
    </section>
  );
};

export default NotFoundPage;
