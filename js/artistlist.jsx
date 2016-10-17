import React from 'react';

class ArtistList extends React.Component {
    render() {
        return (
            <div className="artist-collection">
                {this.props.artists.map((a) => (
                    <div key={a.id} className="artist-item">
                        <img src={a.imageUrl} width="100" height="100" />
                        <p className="artist-name">{a.name}</p>
                    </div>
                ))}
            </div>
        )
    }
}

export default ArtistList;