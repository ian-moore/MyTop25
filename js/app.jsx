import React from 'react'
import ReactDOM from 'react-dom'
import classNames from 'classnames'
import 'whatwg-fetch'
import LoadingIndicator from './loadingindicator.jsx'
import ArtistView from './artistview.jsx'

const defaultState = {
    user: {},
    artists: {},
    artistOrder: [],
    duration: 'medium_term',
    isLoading: true
}

class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = defaultState;
        this.fetchData = this.fetchData.bind(this);
        this.durationChanged = this.durationChanged.bind(this);
    }
    fetchData() {
        var url = `/api/artists?duration=${this.state.duration}`
        fetch(url, { credentials: 'same-origin' })
            .then((response) => { return response.json(); })
            .then((json) => {
                this.setState({
                    user: {
                        name: json.userDisplayName,
                        imageUrl: json.userImage,
                    },
                    artists: json.artists,
                    artistOrder: json.topIds,
                    isLoading: false
                });
            });
    }
    durationChanged(evt) {
        this.setState({
            isLoading: true,
            duration: evt.target.value
        }, this.fetchData);
    }
    componentDidMount() {
        this.fetchData();
    }
    render() {
        var orderedArtists = this.state.artistOrder.map(id => this.state.artists[id]);
        return (
            <div className="container">
                <LoadingIndicator display={this.state.isLoading} />
                <ArtistView display={!this.state.isLoading} 
                    user={this.state.user} 
                    artists={orderedArtists}
                    onDurationChanged={this.durationChanged} />
            </div>
        )
    }
}

ReactDOM.render(
    <App/>, 
    document.getElementById('app')
);