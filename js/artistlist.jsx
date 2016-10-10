import React from 'react';

class ArtistList extends React.Component {
    render() {
        return (
            <div className="artist-collection">
                {this.props.artists.map((a) => (
                    <div key={a.id} className="artist-item">
                        <span>{a.name}</span>
                    </div>
                ))}
            </div>
        )
    }
}

export default ArtistList;