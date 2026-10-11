import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { listEvents } from '../api.js';

export default function EventList() {
  const [events, setEvents] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    listEvents().then((r) => setEvents(r.items), (e) => setError(e.message));
  }, []);

  if (error) return <p className="error">{error}</p>;
  if (!events) return <p>Loading...</p>;

  return (
    <>
      <h1>Events</h1>
      {events.length === 0 && <p>No events yet.</p>}
      {events.map((e) => (
        <Link key={e.id} to={`/events/${e.id}`} className="card">
          <strong>{e.title}</strong>
          <span>{new Date(e.startsAt).toLocaleString()} · {e.location || 'TBA'}</span>
        </Link>
      ))}
    </>
  );
}
