import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { cancelBooking, listBookings } from '../api.js';

export default function MyBookings({ userId }) {
  const [bookings, setBookings] = useState([]);
  const [error, setError] = useState('');

  const load = useCallback(() => {
    if (!userId) return;
    listBookings(userId).then(setBookings, (e) => setError(e.message));
  }, [userId]);

  useEffect(() => { load(); }, [load]);

  const cancel = async (id) => {
    try {
      await cancelBooking(id);
      load();
    } catch (e) {
      setError(e.message);
    }
  };

  if (!userId) return <p>Enter a user id at the top to see your bookings.</p>;

  return (
    <>
      <h1>My bookings</h1>
      {error && <p className="error">{error}</p>}
      {bookings.length === 0 && <p>No bookings.</p>}
      {bookings.map((b) => (
        <div key={b.id} className="card">
          <Link to={`/events/${b.eventId}`}>Event {b.eventId}</Link>
          <span>{b.seats} seat(s) · {new Date(b.createdAt).toLocaleString()}</span>
          <button onClick={() => cancel(b.id)}>Cancel</button>
        </div>
      ))}
    </>
  );
}
